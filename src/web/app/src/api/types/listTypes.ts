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
  importance: boolean;
  dueDate: string | null;   // "YYYY-MM-DD"
  reminder: string | null;  // ISO DateTimeOffset
  repeat: string | null;
  updatedAtUtc: string;
  updatedByMemberId: string | null;
  itemAreaId: string | null;
  targetMemberId: string | null;
}

export interface GetSharedListDetailResponse {
  listId: string;
  name: string;
  kind: string;
  color: string | null;
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
  importance: boolean;
  dueDate: string | null;
  reminder: string | null;
  repeat: string | null;
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
  importance: boolean;
  dueDate: string | null;
  reminder: string | null;
  repeat: string | null;
}

export interface SetItemImportanceRequest {
  importance: boolean;
}

export interface SetItemImportanceResponse {
  itemId: string;
  importance: boolean;
  updatedAtUtc: string;
}

export interface SetItemTemporalRequest {
  dueDate?: string | null;
  reminder?: string | null;
  repeat?: string | null;
}

export interface SetItemTemporalResponse {
  itemId: string;
  dueDate: string | null;
  reminder: string | null;
  repeat: string | null;
  updatedAtUtc: string;
}

export interface ClearItemTemporalResponse {
  itemId: string;
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

export interface UpdateSharedListRequest {
  name?: string | null;
  areaId?: string | null;
  clearArea?: boolean;
  linkedPlanId?: string | null;
  clearLinkedPlan?: boolean;
  kind?: string | null;
  color?: string | null;
  clearColor?: boolean;
}

export interface UpdateSharedListResponse {
  listId: string;
  name: string;
  color: string | null;
  areaId: string | null;
  linkedPlanId: string | null;
  kind: string;
}

export interface SetItemContextRequest {
  itemAreaId?: string | null;
  targetMemberId?: string | null;
}

export interface SetItemContextResponse {
  itemId: string;
  itemAreaId: string | null;
  targetMemberId: string | null;
  updatedAtUtc: string;
}
