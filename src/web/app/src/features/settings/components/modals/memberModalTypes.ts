export interface MemberFormValues {
  name: string;
  role: string;
  birthDate: string;
  isManager: boolean;
}

export interface ProfileFormValues {
  preferredName: string;
  primaryPhone: string;
  primaryEmail: string;
  householdNote: string;
}

export const MEMBER_ROLES = ["Adult", "Child", "Pet"] as const;
export const ADD_MEMBER_ROLES = MEMBER_ROLES;
