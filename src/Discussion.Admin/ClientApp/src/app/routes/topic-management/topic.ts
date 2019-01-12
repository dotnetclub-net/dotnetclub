
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









