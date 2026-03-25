import { request } from "./request";
import type {
  GetFamilySharedListsResponse,
  GetSharedListDetailResponse,
  CreateSharedListRequest,
  CreateSharedListResponse,
  AddItemToSharedListRequest,
  AddItemToSharedListResponse,
  ToggleSharedListItemRequest,
  ToggleSharedListItemResponse,
} from "./types/sharedListTypes";

export const sharedListsApi = {
  getFamilySharedLists: (familyId: string) =>
    request<GetFamilySharedListsResponse>(`/api/shared-lists/family/${familyId}`),

  getSharedListDetail: (listId: string) =>
    request<GetSharedListDetailResponse>(`/api/shared-lists/${listId}`),

  createSharedList: (body: CreateSharedListRequest) =>
    request<CreateSharedListResponse>("/api/shared-lists", {
      method: "POST",
      body: JSON.stringify(body),
    }),

  addItemToSharedList: (listId: string, body: AddItemToSharedListRequest) =>
    request<AddItemToSharedListResponse>(`/api/shared-lists/${listId}/items`, {
      method: "POST",
      body: JSON.stringify(body),
    }),

  toggleSharedListItem: (
    listId: string,
    itemId: string,
    body: ToggleSharedListItemRequest,
  ) =>
    request<ToggleSharedListItemResponse>(
      `/api/shared-lists/${listId}/items/${itemId}/toggle`,
      { method: "POST", body: JSON.stringify(body) },
    ),
};
