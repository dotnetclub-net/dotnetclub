import {TestModuleMetadata} from '@angular/core/testing';
import {APP_BASE_HREF} from '@angular/common';
import {createComponent, mockedHttpBackend, setUpTestBed} from '@testing/common.spec';

import {TopicListComponent} from './topic-list.component';

describe('Component: TopicList', () => {
  setUpTestBed(<TestModuleMetadata>{
    declarations: [TopicListComponent],
    providers: [{ provide: APP_BASE_HREF, useValue: '/' }],
  });

  it('should create topic list component', () => {
    const comp = createComponent(TopicListComponent);
    expect(comp).not.toBeNull();
  });

  it('should load topics on init topic list component', () => {
    const mockedHttp = mockedHttpBackend();
    const comp = createComponent(TopicListComponent);
    comp.ngOnInit();

    const request = mockedHttp.expectOne({method: 'GET', url: 'api/topics?page=1'});
    request.flush( {
        code: 200,
        result: {
          items: [{id: 1, title: 'topic 1'}, {id: 2, title: 'topic 2'}],
          paging: {itemCount: 2, pageSize: 10}
        }
    });

    expect(comp.topics).not.toBeNull();
    expect(comp.topics[0].id).toEqual(1);
    expect(comp.topics[1].id).toEqual(2);
    mockedHttp.verify();
  });

});

