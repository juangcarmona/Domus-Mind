export interface HouseholdAreaItem {
  areaId: string;
  name: string;
  color: string | null;
}

export interface HouseholdAreasResponse {
  areas: HouseholdAreaItem[];
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
