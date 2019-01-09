import {Component, Input, OnInit} from '@angular/core';
import { _HttpClient } from '@delon/theme';
import {Paged, Paging, TopicSummary} from './topic';
import {ApiResponse} from "../../api-response";
import {STChange, STColumn, STPage} from "@delon/abc";
import {NzMessageService} from "ng-zorro-antd";


@Component({
  selector: 'app-topic-list',
  templateUrl: './topic-list.component.html',
})


export class TopicListComponent implements OnInit {
  dotnetClubHostName: string = "localhost:5021";

  topics: TopicSummary[] = [];
  paging: Paging = new Paging();
  loading: boolean = false;
  error: any = null;

  stPagingOptions: STPage = {
    front: false
  };
  columns: STColumn[] = [
    { title: '编号', index: 'id', width: '5%',
      format: (topic: TopicSummary) => {
        return `<a href="https://${this.dotnetClubHostName}/topics/${topic.id}" target="_blank">${topic.id}</a>`
      }
    },
    { title: '标题', index: 'title', width: '60%'},
    {
      title: '作者',
      index: 'author.displayName',
      width: '20%'
    },
    {
      title: '操作',
      width: '15%',
      buttons: [
        {
          text: '删除',
          click: (topic: TopicSummary) => {
            if(!window.confirm(`确实要删除话题 ${topic.title} 吗？`)){
              return;
            }

            this.delete(topic.id, topic.title);
          }
        }
      ],
    },
  ];

  constructor(private httpClient: _HttpClient, private msg: NzMessageService) { }

  listChange( event: STChange){
    if(event.pi !== this.paging.currentPage){
      this.getTopics(event.pi);
    }
  }


  getTopics(page : number){
    this.error = null;
    this.loading = true;

    this.httpClient.get<ApiResponse>('api/topics?page=' + page)
      .subscribe(data => {
        this.loading = false;

        if(data.code === 200){
          const topicResult = <Paged<TopicSummary>>data.result;

          this.topics = topicResult.items;
          this.paging = topicResult.paging;
        }else{
          this.msg.error(data.errorMessage);
        }
      }, (err: any) => {
        this.loading = false;
        this.msg.error(err);
      });
  }

  delete(topicId: number, topicTitle: string){
    this.httpClient.delete('api/topics/' + topicId)
      .subscribe((data: ApiResponse) => {
        if(data.code === 200){
          this.msg.success(`已删除 ${topicTitle}`);
          this.getTopics(0);
        }else{
          this.msg.error(data.errorMessage);
        }
      }, (err: any) => {
        this.msg.error(err);
      });
  }

  ngOnInit() {
    this.getTopics(1);
  }

}
