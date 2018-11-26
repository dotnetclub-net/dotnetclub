using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Discussion.Tests.Common
{
    class ServiceProviderOverrider
    {
        // ReSharper disable AssignNullToNotNullAttribute
        // ReSharper disable PossibleNullReferenceException
        
        private readonly List<ServiceDescriptor> _originalDescriptors;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, object> _builtInItems = new Dictionary<Type, object>();
        private bool _hasOverriden;
        
        
        public ServiceProviderOverrider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            InitReflection();
            
            var (callSiteFactory, _) = GetCallSiteFromServiceProvider();
            _originalDescriptors = _descriptorsField.GetValue(callSiteFactory) as List<ServiceDescriptor>;

            var callSiteCache = _callSiteCacheField.GetValue(callSiteFactory) as IDictionary;
            _builtInItems.Add(typeof (IServiceProvider), callSiteCache[typeof (IServiceProvider)]);
            _builtInItems.Add(typeof (IServiceScopeFactory), callSiteCache[typeof (IServiceScopeFactory)]);
        }

          
        public void OverrideService(IList<ServiceDescriptor> services)
        {
            _hasOverriden = true;
            var (callSiteFactory, realizedService) = GetCallSiteFromServiceProvider();

            realizedService.Clear();
            var callSiteCache = _callSiteCacheField.GetValue(callSiteFactory) as IDictionary;
            callSiteCache.Clear();

            var updatedDescriptors = new List<ServiceDescriptor>(_originalDescriptors);
            updatedDescriptors.AddRange(services);

            _descriptorsField.SetValue(callSiteFactory, updatedDescriptors);

            var descriptorLookup = _descriptorLookupField.GetValue(callSiteFactory) as IDictionary;
            descriptorLookup.Clear();
            _populateMethod.Invoke(callSiteFactory, new []{ (object) updatedDescriptors  });

            foreach (var key in _builtInItems.Keys)
            {
                callSiteCache.Add(key, _builtInItems[key]);    
            }
        }

        public void Restore()
        {
            if (!_hasOverriden)
            {
                return;
            }
            
            OverrideService(_originalDescriptors);
            _hasOverriden = false;
        }
        
        private Type _spType;
        private Type _engineType;
        private Type _callSiteFactoryType;
        private FieldInfo _spEngineField;
        private FieldInfo _descriptorsField;
        private PropertyInfo _spCallSiteFactoryProp;
        private PropertyInfo _spRealizedServicesProp;
        private FieldInfo _callSiteCacheField;
        private FieldInfo _descriptorLookupField;
        private MethodInfo _populateMethod;

        private const BindingFlags InternalMember = BindingFlags.Instance | BindingFlags.NonPublic;
        
        void InitReflection()
        {
            _spType = typeof(ServiceProvider);
            _spEngineField = _spType.GetField("_engine", InternalMember);

            var engine = _spEngineField.GetValue(_serviceProvider);
            _engineType = engine.GetType();
            _spRealizedServicesProp = _engineType.GetProperty("RealizedServices", InternalMember);
            _spCallSiteFactoryProp = _engineType.GetProperty("CallSiteFactory", InternalMember);
            
            
            var callSiteFactory = _spCallSiteFactoryProp.GetValue(engine);
            _callSiteFactoryType = callSiteFactory.GetType();
            
            _descriptorsField = _callSiteFactoryType.GetField("_descriptors", InternalMember);
            _callSiteCacheField = _callSiteFactoryType.GetField("_callSiteCache", InternalMember);
            _descriptorLookupField = _callSiteFactoryType.GetField("_descriptorLookup", InternalMember);
            _populateMethod = _callSiteFactoryType.GetMethod("Populate", InternalMember);
        }
        
        (object, IDictionary) GetCallSiteFromServiceProvider()
        {
            var engine = _spEngineField.GetValue(_serviceProvider);
            
            var callSiteFactory = _spCallSiteFactoryProp.GetValue(engine);
            var realizedService = _spRealizedServicesProp.GetValue(engine) as IDictionary;
            return (callSiteFactory, realizedService);
        }
        
        
    }
}