export interface UserData {
  id: number;
  email: string;
  userName: string;
  phoneNumber: any;
  phoneRegionCode: any;
  image: Image;
  banned: boolean;
  isExternal: boolean;
  isEmailConfirmed: boolean;
  createdAt: string;
  roles: string[];
}
interface Image {
  name: string;
  imageURL: string;
  hash: number;
  version: string;
}
