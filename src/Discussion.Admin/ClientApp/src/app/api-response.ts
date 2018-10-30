
export class ApiResponse {
  code: number;
  hasSucceeded: boolean;
  errorMessage: string;
  errors: {
    [key: string]: string[] | null;
  };
  result: any;
}
