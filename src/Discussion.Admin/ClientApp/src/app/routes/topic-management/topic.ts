export class Topic {
  id: number;
  title: string;
  content: string;
}

export class TopicSummary {
  id: number;
  title: string;
  createdAt: Date;
  author: TopicAuthorSummary;
}

export class TopicAuthorSummary {
  id: number;
  displayName: string;
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


