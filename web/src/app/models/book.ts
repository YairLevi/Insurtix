export interface Book {
  isbn: string;
  title: string;
  authors: string[];
  category: string;
  cover: string | null;
  year: number;
  price: number;
}
