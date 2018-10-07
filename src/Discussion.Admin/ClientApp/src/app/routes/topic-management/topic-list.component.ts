import { Component, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import {Topic} from "./topic";



@Component({
  selector: 'app-topic-list',
  templateUrl: './topic-list.component.html',
})


export class TopicListComponent implements OnInit {
  topics: Array<Topic>;
  constructor(private httpClient: _HttpClient) { }




  ngOnInit() {
    this.httpClient.get<Array<Topic>>('api/topics')
                   .subscribe(data => { this.topics = data; });

  }

}




