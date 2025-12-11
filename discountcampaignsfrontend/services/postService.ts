// services/postService.ts
import { DiscountRequest } from '@/app/page';
import api from '../lib/api';

export interface Product {
    name: string;
    category: string;
    price: number;
}

export async function getProducts(): Promise<Product[]> {
  const res = await api.get<Product[]>('/Products/GetProducts');
  return res.data;
}

export interface DiscountResponse {
  original: number;
  final: number;
  applied: Array<{ code: string; amount: number }>;
}

export async function calculateTotalSum( discountRequestDto : DiscountRequest): Promise<DiscountResponse> {
  const res = await api.post<DiscountResponse>('/Products/CalculateTotalSum', discountRequestDto);
  return res.data;
}
