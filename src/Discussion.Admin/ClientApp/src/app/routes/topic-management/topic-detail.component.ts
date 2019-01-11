import {Component, NgZone, OnInit} from '@angular/core';
import { _HttpClient } from '@delon/theme';
import {Paged, Reply, TopicDetail } from './topic';
import {ApiResponse} from "../../api-response";
import {NzMessageService} from "ng-zorro-antd";
import {ActivatedRoute, Router} from '@angular/router';


@Component({
  selector: 'app-topic-detail',
  templateUrl: './topic-detail.component.html',
})


export class TopicDetailComponent implements OnInit {

  topic: TopicDetail;
  topicId: string;
  replies: Reply[];

  constructor(private _httpClient: _HttpClient,
              private _msg: NzMessageService,
              private _route:ActivatedRoute,
              private router: Router) { }



  getTopicDetail(){
    this._httpClient.get<ApiResponse>('api/topics/' + this.topicId)
      .subscribe(data => {
        if(data.code === 200){
          this.topic = <TopicDetail>data.result;
        }else{
          this._msg.error(data.errorMessage);
        }
      }, (err: any) => {
        this.backToList();
        this._msg.error(err);
      });
  }

  getReplyList(){
    this._httpClient.get<ApiResponse>(`api/topics/${this.topicId}/replies`)
      .subscribe(data => {
        if(data.code === 200){
          this.replies = <Reply[]>data.result;
        }else{
          this._msg.error(data.errorMessage);
        }
      }, (err: any) => {
        this._msg.error(err);
      });
  }

  deleteTopic(topicId: number, topicTitle: string){
    if(!window.confirm(`确定要删除此话题吗？\n ${topicTitle}`)){
      return;
    }

    this._httpClient.delete(`api/topics/${this.topicId}`)
      .subscribe((data: ApiResponse) => {
        if(data.code === 200){
          this.backToList();
          this._msg.success(`已删除 ${topicTitle}`);
        }else{
          this._msg.error(data.errorMessage);
        }
      }, (err: any) => {
        this._msg.error(err);
      });
  }

  deleteReply(replyId: number){
    if(!window.confirm('确定要删除这条回复吗？')){
      return;
    }


    this._httpClient.delete(`api/topics/${this.topicId}/replies/${replyId}`)
      .subscribe((data: ApiResponse) => {
        if(data.code === 200){
          this.getReplyList();
          this._msg.success(`回复 ${replyId} 已删除。`);
        }else{
          this._msg.error(data.errorMessage);
        }
      }, (err: any) => {
        this._msg.error(err);
      });
  }

  backToList(){
    this.router.navigate(['/topics'])
  }

  ngOnInit() {
    this.topicId = this._route.snapshot.paramMap.get('id');

    this.getTopicDetail();
    this.getReplyList();
  }

}
