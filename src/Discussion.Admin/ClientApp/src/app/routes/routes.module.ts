import { NgModule } from '@angular/core';
import { SharedModule } from '@shared/shared.module';
import { Routes, RouterModule } from '@angular/router';
import { environment } from '@env/environment';

// layout
import { LayoutDefaultComponent } from '../layout/default/default.component';
import { LayoutPassportComponent } from '../layout/passport/passport.component';
// dashboard pages
import { DashboardComponent } from './dashboard/dashboard.component';
// passport pages
import { UserLoginComponent } from './passport/login/login.component';
import { UserRegisterComponent } from './passport/register/register.component';
// single pages
import { Exception403Component } from './exception/403.component';
import { Exception404Component } from './exception/404.component';
import { Exception500Component } from './exception/500.component';

// dotnetclub components
import { TopicListComponent } from './topic-management/topic-list.component';
import { TopicDetailComponent } from './topic-management/topic-detail.component';
import {UserListComponent} from "./user-management/user-list.component";



const routes: Routes = [
  {
    path: '',
    component: LayoutDefaultComponent,
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent, data: { title: '管理面板' } },
      { path: 'topics/:id', component: TopicDetailComponent, data: { title: '话题详情'} },
      { path: 'topics', component: TopicListComponent, data: { title: '话题管理'} },
      { path: 'users', component: UserListComponent, data: { title: '用户管理'} },
    ]
  },
  {
    path: 'passport',
    component: LayoutPassportComponent,
    children: [
      { path: 'login', component: UserLoginComponent, data: { title: '登录管理面板'} },
      { path: 'register', component: UserRegisterComponent, data: { title: '注册管理员'} },
    ]
  },
  { path: '403', component: Exception403Component, data: { title: '没有操作权限'}},
  { path: '404', component: Exception404Component, data: { title: '找不到资源'} },
  { path: '500', component: Exception500Component, data: { title: '发生了错误'} },
  { path: '**', redirectTo: 'dashboard' }
];



const COMPONENTS = [
  DashboardComponent,

  UserLoginComponent,
  UserRegisterComponent,

  Exception403Component,
  Exception404Component,
  Exception500Component,

  TopicListComponent,
  TopicDetailComponent,

  UserListComponent
];
const COMPONENTS_NOROUNT = [];

@NgModule({
  imports: [ SharedModule, RouterModule.forRoot(routes, { useHash: environment.useHash }) ],
  exports: [RouterModule],
  declarations: [
    ...COMPONENTS,
    ...COMPONENTS_NOROUNT
  ],
  entryComponents: COMPONENTS_NOROUNT
})
export class RoutesModule {}
