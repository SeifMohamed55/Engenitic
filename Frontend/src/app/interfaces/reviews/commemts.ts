export interface Comments {
  totalPages: number;
  totalItems: number;
  currentlyViewing: string;
  pageIndex: number;
  paginatedList: PaginatedList[];
}

interface PaginatedList {
  reviewId: number;
  userId: number;
  courseId: number;
  content: string;
  updatedAt: string;
  rating: number;
  fullName: string;
  imageMetadata: ImageMetadata;
}

interface ImageMetadata {
  name: string;
  imageURL: string;
  hash: number;
  version: string;
}
