export interface SharedListSummary {
  id: string;
  name: string;
  kind: string;
  areaId: string | null;
  linkedEntityType: string | null;
  linkedEntityId: string | null;
  itemCount: number;
  uncheckedCount: number;
}

export interface GetFamilySharedListsResponse {
  lists: SharedListSummary[];
}

export interface SharedListItemDetail {
  itemId: string;
  name: string;
  checked: boolean;
  quantity: string | null;
  note: string | null;
  order: number;
  updatedAtUtc: string;
  updatedByMemberId: string | null;
}

export interface GetSharedListDetailResponse {
  listId: string;
  name: string;
  kind: string;
  areaId: string | null;
  linkedEntityType: string | null;
  linkedEntityId: string | null;
  linkedEntityDisplayName: string | null;
  items: SharedListItemDetail[];
}

export interface CreateSharedListRequest {
  familyId: string;
  name: string;
  kind: string;
  areaId?: string | null;
  linkedEntityType?: string | null;
  linkedEntityId?: string | null;
}

export interface CreateSharedListResponse {
  listId: string;
  familyId: string;
  name: string;
  kind: string;
  areaId: string | null;
  linkedEntityType: string | null;
  linkedEntityId: string | null;
  createdAtUtc: string;
}

export interface AddItemToSharedListRequest {
  name: string;
  quantity?: string | null;
  note?: string | null;
}

export interface AddItemToSharedListResponse {
  itemId: string;
  listId: string;
  name: string;
  checked: boolean;
  quantity: string | null;
  note: string | null;
  order: number;
}

export interface ToggleSharedListItemRequest {
  updatedByMemberId?: string | null;
}

export interface ToggleSharedListItemResponse {
  itemId: string;
  checked: boolean;
  updatedAtUtc: string;
  updatedByMemberId: string | null;
  uncheckedCount: number;
}

export interface UpdateSharedListItemRequest {
  name: string;
  quantity?: string | null;
  note?: string | null;
}

export interface UpdateSharedListItemResponse {
  itemId: string;
  name: string;
  quantity: string | null;
  note: string | null;
  updatedAtUtc: string;
}

export interface LinkSharedListRequest {
  linkedEntityType: string;
  linkedEntityId: string;
}

export interface LinkSharedListResponse {
  listId: string;
  linkedEntityType: string;
  linkedEntityId: string;
}

export interface CreateLinkedSharedListForEventRequest {
  familyId: string;
  name?: string | null;
}

export interface GetSharedListByLinkedEntityResponse {
  listId: string;
  name: string;
  kind: string;
  itemCount: number;
  uncheckedCount: number;
}

export interface RenameSharedListRequest {
  name: string;
}

export interface RenameSharedListResponse {
  listId: string;
  name: string;
}

export interface ReorderSharedListItemsRequest {
  itemIds: string[];
}
