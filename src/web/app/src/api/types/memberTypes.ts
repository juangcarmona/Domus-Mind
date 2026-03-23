export interface ApiError {
  status: number;
  message: string;
}

export interface FamilyResponse {
  familyId: string;
  name: string;
  primaryLanguageCode: string | null;
  createdAtUtc: string;
  memberCount: number;
  firstDayOfWeek: string | null;
  dateFormatPreference: string | null;
}

export interface CreateFamilyRequest {
  name: string;
  primaryLanguageCode?: string | null;
}

export interface SupportedLanguageItem {
  code: string;
  culture: string;
  displayName: string;
  nativeDisplayName: string;
  isDefault: boolean;
  sortOrder: number;
}

export type MemberAccessStatus =
  | "NoAccess"
  | "InvitedOrProvisioned"
  | "PasswordResetRequired"
  | "Active"
  | "Disabled";

export interface FamilyMemberResponse {
  memberId: string;
  familyId: string;
  name: string;
  /** Optional preferred display name. When set, the UI uses this instead of name. */
  preferredName: string | null;
  role: string;
  isManager: boolean;
  birthDate: string | null;
  joinedAtUtc: string;
  authUserId: string | null;
  accessStatus: MemberAccessStatus;
  linkedEmail: string | null;
  /** True when this member is the currently authenticated user. */
  isCurrentUser: boolean;
  /** True when the member has a linked login account. */
  hasAccount: boolean;
  /** True when the requesting user (a manager) can provision access for this member. */
  canGrantAccess: boolean;
  /** True when the requesting user may edit this member. */
  canEdit: boolean;
  /** First letter of the effective display name, upper-cased, for the avatar placeholder. */
  avatarInitial: string;
}

export interface AddMemberRequest {
  name: string;
  role: string;
  birthDate?: string | null;
  isManager?: boolean;
}

export interface AddMemberResponse {
  memberId: string;
  familyId: string;
  name: string;
  role: string;
  joinedAtUtc: string;
}

export interface LinkMemberAccountRequest {
  username: string;
  temporaryPassword: string;
}

export interface ProvisionMemberAccessRequest {
  email: string;
  displayName?: string | null;
}

export interface ProvisionMemberAccessResponse {
  userId: string;
  memberId: string;
  email: string;
  temporaryPassword: string;
  mustChangePassword: boolean;
}

export interface RegenerateTemporaryPasswordResponse {
  temporaryPassword: string;
  mustChangePassword: boolean;
}

export interface DisableMemberAccessResponse {
  memberId: string;
}

export interface EnableMemberAccessResponse {
  memberId: string;
}

export interface LinkMemberAccountResponse {
  memberId: string;
  familyId: string;
  name: string;
  role: string;
  isManager: boolean;
  birthDate: string | null;
  username: string;
  authUserId: string;
  linkedAtUtc: string;
}

export interface InviteMemberRequest {
  name: string;
  role: string;
  birthDate?: string | null;
  isManager: boolean;
  username: string;
  temporaryPassword: string;
}

export interface InviteMemberResponse {
  memberId: string;
  familyId: string;
  name: string;
  role: string;
  isManager: boolean;
  birthDate: string | null;
  username: string;
  joinedAtUtc: string;
}

export interface UpdateMemberRequest {
  name: string;
  role: string;
  birthDate?: string | null;
  isManager: boolean;
}

export interface UpdateMemberResponse {
  memberId: string;
  familyId: string;
  name: string;
  role: string;
  isManager: boolean;
  birthDate: string | null;
  joinedAtUtc: string;
}

/** Phase 2 member detail — includes profile seam fields. */
export interface MemberDetailResponse extends FamilyMemberResponse {
  lastLoginAtUtc: string | null;
  primaryPhone: string | null;
  primaryEmail: string | null;
  householdNote: string | null;
}

export interface UpdateMemberProfileRequest {
  preferredName?: string | null;
  primaryPhone?: string | null;
  primaryEmail?: string | null;
  householdNote?: string | null;
}

export interface UpdateMemberProfileResponse {
  memberId: string;
  familyId: string;
  preferredName: string | null;
  primaryPhone: string | null;
  primaryEmail: string | null;
  householdNote: string | null;
}

export interface UpdateFamilySettingsRequest {
  name: string;
  primaryLanguageCode?: string | null;
  firstDayOfWeek?: string | null;
  dateFormatPreference?: string | null;
}

export interface UpdateFamilySettingsResponse {
  familyId: string;
  name: string;
  primaryLanguageCode: string | null;
  firstDayOfWeek: string | null;
  dateFormatPreference: string | null;
}

export interface AdditionalMemberRequest {
  name: string;
  birthDate?: string | null;
  type?: string | null;
  manager?: boolean;
}

export interface CompleteOnboardingRequest {
  selfName: string;
  selfBirthDate?: string | null;
  additionalMembers?: AdditionalMemberRequest[];
}

export interface OnboardingMemberItem {
  memberId: string;
  name: string;
  role: string;
  isManager: boolean;
  birthDate: string | null;
  joinedAtUtc: string;
}

export interface CompleteOnboardingResponse {
  familyId: string;
  familyName: string;
  members: OnboardingMemberItem[];
}
