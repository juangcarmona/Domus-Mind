import { request } from "./request";
import type {
  FamilyResponse,
  CreateFamilyRequest,
  FamilyMemberResponse,
  AddMemberRequest,
  AddMemberResponse,
  InviteMemberRequest,
  InviteMemberResponse,
  LinkMemberAccountRequest,
  LinkMemberAccountResponse,
  ProvisionMemberAccessRequest,
  ProvisionMemberAccessResponse,
  RegenerateTemporaryPasswordResponse,
  DisableMemberAccessResponse,
  EnableMemberAccessResponse,
  MemberDetailResponse,
  UpdateMemberRequest,
  UpdateMemberResponse,
  UpdateMemberProfileRequest,
  UpdateMemberProfileResponse,
  UpdateFamilySettingsRequest,
  UpdateFamilySettingsResponse,
  CompleteOnboardingRequest,
  CompleteOnboardingResponse,
  SupportedLanguageItem,
} from "./types/memberTypes";
import type {
  EnrichedTimelineResponse,
} from "./types/calendarTypes";

export const familyApi = {
  createFamily: (body: CreateFamilyRequest) =>
    request<FamilyResponse>("/api/families", { method: "POST", body: JSON.stringify(body) }),

  getMyFamily: () =>
    request<FamilyResponse>("/api/families/mine"),

  getFamily: (familyId: string) =>
    request<FamilyResponse>(`/api/families/${familyId}`),

  getMembers: (familyId: string) =>
    request<FamilyMemberResponse[]>(`/api/families/${familyId}/members`),

  addMember: (familyId: string, body: AddMemberRequest) =>
    request<AddMemberResponse>(`/api/families/${familyId}/members`, { method: "POST", body: JSON.stringify(body) }),

  inviteMember: (familyId: string, body: InviteMemberRequest) =>
    request<InviteMemberResponse>(`/api/families/${familyId}/members/invite`, { method: "POST", body: JSON.stringify(body) }),

  linkMemberAccount: (familyId: string, memberId: string, body: LinkMemberAccountRequest) =>
    request<LinkMemberAccountResponse>(`/api/families/${familyId}/members/${memberId}/link-account`, { method: "POST", body: JSON.stringify(body) }),

  provisionMemberAccess: (familyId: string, memberId: string, body: ProvisionMemberAccessRequest) =>
    request<ProvisionMemberAccessResponse>(`/api/families/${familyId}/members/${memberId}/provision-access`, { method: "POST", body: JSON.stringify(body) }),

  regeneratePassword: (familyId: string, memberId: string) =>
    request<RegenerateTemporaryPasswordResponse>(`/api/families/${familyId}/members/${memberId}/regenerate-password`, { method: "POST" }),

  disableMemberAccess: (familyId: string, memberId: string) =>
    request<DisableMemberAccessResponse>(`/api/families/${familyId}/members/${memberId}/disable-access`, { method: "POST" }),

  enableMemberAccess: (familyId: string, memberId: string) =>
    request<EnableMemberAccessResponse>(`/api/families/${familyId}/members/${memberId}/enable-access`, { method: "POST" }),

  getMemberDetails: (familyId: string, memberId: string) =>
    request<MemberDetailResponse>(`/api/families/${familyId}/members/${memberId}`),

  updateMember: (familyId: string, memberId: string, body: UpdateMemberRequest) =>
    request<UpdateMemberResponse>(`/api/families/${familyId}/members/${memberId}`, { method: "PUT", body: JSON.stringify(body) }),

  updateMemberProfile: (familyId: string, memberId: string, body: UpdateMemberProfileRequest) =>
    request<UpdateMemberProfileResponse>(`/api/families/${familyId}/members/${memberId}/profile`, { method: "PATCH", body: JSON.stringify(body) }),

  completeOnboarding: (familyId: string, body: CompleteOnboardingRequest) =>
    request<CompleteOnboardingResponse>(`/api/families/${familyId}/onboarding`, { method: "POST", body: JSON.stringify(body) }),

  updateFamilySettings: (familyId: string, body: UpdateFamilySettingsRequest) =>
    request<UpdateFamilySettingsResponse>(`/api/families/${familyId}/settings`, { method: "PUT", body: JSON.stringify(body) }),

  getEnrichedTimeline: (familyId: string, opts?: { types?: string; memberFilter?: string; from?: string; to?: string; statuses?: string }) => {
    const params = new URLSearchParams();
    if (opts?.types) params.set("types", opts.types);
    if (opts?.memberFilter) params.set("memberFilter", opts.memberFilter);
    if (opts?.from) params.set("from", opts.from);
    if (opts?.to) params.set("to", opts.to);
    if (opts?.statuses) params.set("statuses", opts.statuses);
    const qs = params.toString();
    return request<EnrichedTimelineResponse>(`/api/families/${familyId}/timeline/enriched${qs ? `?${qs}` : ""}`);
  },

  getSupportedLanguages: () =>
    request<{ languages: SupportedLanguageItem[] }>("/api/languages", {}, null),
};
