export interface Login {
    status: string
    code: number
    message: string
    data: Data
};

interface Data {
    name: string
    roles: string[]
    accessToken: string
    validTo: string
    image: string
};