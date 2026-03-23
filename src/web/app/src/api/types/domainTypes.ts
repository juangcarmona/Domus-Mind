export interface HouseholdAreaItem {
  areaId: string;
  name: string;
  primaryOwnerId: string | null;
  primaryOwnerName: string | null;
  secondaryOwnerIds: string[];
}

export interface HouseholdAreasResponse {
  areas: HouseholdAreaItem[];
}

export interface CreateResponsibilityDomainRequest {
  name: string;
  familyId: string;
}

export interface CreateResponsibilityDomainResponse {
  responsibilityDomainId: string;
  familyId: string;
  name: string;
  createdAtUtc: string;
}

export interface AssignPrimaryOwnerRequest {
  memberId: string;
}

export interface AssignSecondaryOwnerRequest {
  memberId: string;
}

export interface TransferResponsibilityRequest {
  newPrimaryOwnerId: string;
}

export interface CreateTaskRequest {
  title: string;
  familyId: string;
  description?: string;
  dueDate?: string | null;
  dueTime?: string | null;
  color?: string;
}

export interface CreateTaskResponse {
  taskId: string;
  familyId: string;
  title: string;
  description: string | null;
  dueDate: string | null;
  status: string;
  color: string;
  createdAtUtc: string;
}

export interface AssignTaskRequest {
  assigneeId: string;
}
