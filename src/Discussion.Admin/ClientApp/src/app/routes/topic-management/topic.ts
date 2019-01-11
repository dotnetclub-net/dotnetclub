
export class TopicSummary {
  id: number;
  title: string;
  createdAt: Date;
  author: AuthorSummary;
  viewCount: number;
  replyCount: number;
}

export class AuthorSummary {
  id: number;
  displayName: string;
}

export class TopicDetail extends  TopicSummary {
  markdownContent :string;
  htmlContent : string;
}

export class Reply {
  id: number;
  markdownContent :string;
  htmlContent : string;
  author: AuthorSummary
}













export class Paged<T> {
  items: T[];
  paging: Paging;
}

export class Paging {
  pageSize: number = 20;
  itemCount: number = 0;
  currentPage: number = 1;
  totalPages: number = 0;

  hasNextPage: boolean = false;
  hasPreviousPage: boolean = false;
}


