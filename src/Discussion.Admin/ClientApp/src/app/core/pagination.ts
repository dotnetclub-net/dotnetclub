
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

