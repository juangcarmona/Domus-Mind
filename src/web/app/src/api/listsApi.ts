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
  UpdateSharedListItemRequest,
  UpdateSharedListItemResponse,
  SetItemImportanceRequest,
  SetItemImportanceResponse,
  SetItemTemporalRequest,
  SetItemTemporalResponse,
  ClearItemTemporalResponse,
  LinkSharedListRequest,
  LinkSharedListResponse,
  CreateLinkedSharedListForEventRequest,
  GetSharedListByLinkedEntityResponse,
  RenameSharedListRequest,
  RenameSharedListResponse,
  ReorderSharedListItemsRequest,
  UpdateSharedListRequest,
  UpdateSharedListResponse,
  SetItemContextRequest,
  SetItemContextResponse,
} from "./types/listTypes";

export const listsApi = {
  getFamilyLists: (familyId: string) =>
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

  updateSharedListItem: (listId: string, itemId: string, body: UpdateSharedListItemRequest) =>
    request<UpdateSharedListItemResponse>(
      `/api/shared-lists/${listId}/items/${itemId}`,
      { method: "PATCH", body: JSON.stringify(body) },
    ),

  removeSharedListItem: (listId: string, itemId: string) =>
    request<void>(`/api/shared-lists/${listId}/items/${itemId}`, { method: "DELETE" }),

  linkSharedList: (listId: string, body: LinkSharedListRequest) =>
    request<LinkSharedListResponse>(`/api/shared-lists/${listId}/link`, {
      method: "PATCH",
      body: JSON.stringify(body),
    }),

  unlinkSharedList: (listId: string) =>
    request<void>(`/api/shared-lists/${listId}/link`, { method: "DELETE" }),

  createLinkedSharedListForEvent: (eventId: string, body: CreateLinkedSharedListForEventRequest) =>
    request<CreateSharedListResponse>(`/api/shared-lists/linked/calendar-event/${eventId}`, {
      method: "POST",
      body: JSON.stringify(body),
    }),

  getSharedListByLinkedEntity: (entityType: string, entityId: string) =>
    request<GetSharedListByLinkedEntityResponse>(
      `/api/shared-lists/by-entity/${entityType}/${entityId}`,
    ),

  renameSharedList: (listId: string, body: RenameSharedListRequest) =>
    request<RenameSharedListResponse>(`/api/shared-lists/${listId}/name`, {
      method: "PATCH",
      body: JSON.stringify(body),
    }),

  deleteSharedList: (listId: string) =>
    request<void>(`/api/shared-lists/${listId}`, { method: "DELETE" }),

  updateSharedList: (listId: string, body: UpdateSharedListRequest) =>
    request<UpdateSharedListResponse>(`/api/shared-lists/${listId}`, {
      method: "PATCH",
      body: JSON.stringify(body),
    }),

  reorderSharedListItems: (listId: string, body: ReorderSharedListItemsRequest) =>
    request<void>(`/api/shared-lists/${listId}/items/order`, {
      method: "PATCH",
      body: JSON.stringify(body),
    }),

  setItemImportance: (listId: string, itemId: string, body: SetItemImportanceRequest) =>
    request<SetItemImportanceResponse>(
      `/api/shared-lists/${listId}/items/${itemId}/importance`,
      { method: "PATCH", body: JSON.stringify(body) },
    ),

  setItemTemporal: (listId: string, itemId: string, body: SetItemTemporalRequest) =>
    request<SetItemTemporalResponse>(
      `/api/shared-lists/${listId}/items/${itemId}/temporal`,
      { method: "PATCH", body: JSON.stringify(body) },
    ),

  clearItemTemporal: (listId: string, itemId: string) =>
    request<ClearItemTemporalResponse>(
      `/api/shared-lists/${listId}/items/${itemId}/temporal`,
      { method: "DELETE" },
    ),

  setItemContext: (listId: string, itemId: string, body: SetItemContextRequest) =>
    request<SetItemContextResponse>(
      `/api/shared-lists/${listId}/items/${itemId}/context`,
      { method: "PATCH", body: JSON.stringify(body) },
    ),
};
