export interface LoginData {
  id: number;
  banned: boolean;
  name: string;
  roles: string[];
  validTo: string;
  image: Image;
  accessToken: string;
}

export interface Image {
  name: string;
  imageURL: string;
  hash: number;
  version: any;
}
