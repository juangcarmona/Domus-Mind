import { createAsyncThunk } from "@reduxjs/toolkit";
import {
  domusmindApi,
  type AdditionalMemberRequest,
  type UpdateFamilySettingsRequest,
  type InviteMemberRequest,
  type LinkMemberAccountRequest,
  type ProvisionMemberAccessRequest,
  type UpdateMemberRequest,
  type UpdateMemberProfileRequest,
} from "../api/domusmindApi";

const FAMILY_KEY = "dm_family_id";

export function getStoredFamilyId(): string | null {
  return localStorage.getItem(FAMILY_KEY);
}

export function storeFamilyId(id: string): void {
  localStorage.setItem(FAMILY_KEY, id);
}

export function clearStoredFamilyId(): void {
  localStorage.removeItem(FAMILY_KEY);
}

export const fetchMembers = createAsyncThunk(
  "household/fetchMembers",
  async (familyId: string, { rejectWithValue }) => {
    try {
      return await domusmindApi.getMembers(familyId);
    } catch (err: unknown) {
      return rejectWithValue((err as { message?: string }).message ?? "Failed to load people");
    }
  },
);

export const bootstrapHousehold = createAsyncThunk(
  "household/bootstrap",
  async (_) => {
    try {
      const family = await domusmindApi.getMyFamily();
      const members = await domusmindApi.getMembers(family.familyId);
      storeFamilyId(family.familyId);
      return { family, members };
    } catch {
      return null;
    }
  },
);

export const createFamily = createAsyncThunk(
  "household/createFamily",
  async (
    payload: { name: string; primaryLanguageCode?: string | null },
    { rejectWithValue },
  ) => {
    try {
      const family = await domusmindApi.createFamily({
        name: payload.name,
        primaryLanguageCode: payload.primaryLanguageCode,
      });
      storeFamilyId(family.familyId);
      return family;
    } catch (err: unknown) {
      return rejectWithValue((err as { message?: string }).message ?? "Failed to create household");
    }
  },
);

export const addMember = createAsyncThunk(
  "household/addMember",
  async (
    { familyId, name, role, birthDate, isManager }: { familyId: string; name: string; role: string; birthDate?: string | null; isManager?: boolean },
    { dispatch, rejectWithValue },
  ) => {
    try {
      const response = await domusmindApi.addMember(familyId, { name, role, birthDate, isManager });
      dispatch(fetchMembers(familyId));
      return response;
    } catch (err: unknown) {
      return rejectWithValue((err as { message?: string }).message ?? "Failed to add person");
    }
  },
);

export const inviteMember = createAsyncThunk(
  "household/inviteMember",
  async (
    { familyId, ...body }: { familyId: string } & InviteMemberRequest,
    { dispatch, rejectWithValue },
  ) => {
    try {
      const response = await domusmindApi.inviteMember(familyId, body);
      dispatch(fetchMembers(familyId));
      return response;
    } catch (err: unknown) {
      return rejectWithValue((err as { message?: string }).message ?? "Failed to invite member");
    }
  },
);

export const linkMemberAccount = createAsyncThunk(
  "household/linkMemberAccount",
  async (
    { familyId, memberId, ...body }: { familyId: string; memberId: string } & LinkMemberAccountRequest,
    { dispatch, rejectWithValue },
  ) => {
    try {
      const response = await domusmindApi.linkMemberAccount(familyId, memberId, body);
      dispatch(fetchMembers(familyId));
      return response;
    } catch (err: unknown) {
      return rejectWithValue((err as { message?: string }).message ?? "Failed to link account");
    }
  },
);

export const provisionMemberAccess = createAsyncThunk(
  "household/provisionMemberAccess",
  async (
    { familyId, memberId, ...body }: { familyId: string; memberId: string } & ProvisionMemberAccessRequest,
    { dispatch, rejectWithValue },
  ) => {
    try {
      const response = await domusmindApi.provisionMemberAccess(familyId, memberId, body);
      dispatch(fetchMembers(familyId));
      return response;
    } catch (err: unknown) {
      return rejectWithValue((err as { message?: string }).message ?? "Failed to provision access");
    }
  },
);

export const regeneratePassword = createAsyncThunk(
  "household/regeneratePassword",
  async (
    { familyId, memberId }: { familyId: string; memberId: string },
    { rejectWithValue },
  ) => {
    try {
      return await domusmindApi.regeneratePassword(familyId, memberId);
    } catch (err: unknown) {
      return rejectWithValue((err as { message?: string }).message ?? "Failed to regenerate password");
    }
  },
);

export const disableMemberAccess = createAsyncThunk(
  "household/disableMemberAccess",
  async (
    { familyId, memberId }: { familyId: string; memberId: string },
    { dispatch, rejectWithValue },
  ) => {
    try {
      const response = await domusmindApi.disableMemberAccess(familyId, memberId);
      dispatch(fetchMembers(familyId));
      return response;
    } catch (err: unknown) {
      return rejectWithValue((err as { message?: string }).message ?? "Failed to disable access");
    }
  },
);

export const enableMemberAccess = createAsyncThunk(
  "household/enableMemberAccess",
  async (
    { familyId, memberId }: { familyId: string; memberId: string },
    { dispatch, rejectWithValue },
  ) => {
    try {
      const response = await domusmindApi.enableMemberAccess(familyId, memberId);
      dispatch(fetchMembers(familyId));
      return response;
    } catch (err: unknown) {
      return rejectWithValue((err as { message?: string }).message ?? "Failed to enable access");
    }
  },
);

export const updateMemberProfile = createAsyncThunk(
  "household/updateMemberProfile",
  async (
    { familyId, memberId, ...body }: { familyId: string; memberId: string } & UpdateMemberProfileRequest,
    { dispatch, rejectWithValue },
  ) => {
    try {
      const response = await domusmindApi.updateMemberProfile(familyId, memberId, body);
      dispatch(fetchMembers(familyId));
      return response;
    } catch (err: unknown) {
      return rejectWithValue((err as { message?: string }).message ?? "Failed to update profile");
    }
  },
);

export const updateMember = createAsyncThunk(
  "household/updateMember",
  async (
    { familyId, memberId, ...body }: { familyId: string; memberId: string } & UpdateMemberRequest,
    { rejectWithValue },
  ) => {
    try {
      return await domusmindApi.updateMember(familyId, memberId, body);
    } catch (err: unknown) {
      return rejectWithValue((err as { message?: string }).message ?? "Failed to update member");
    }
  },
);

export const completeOnboarding = createAsyncThunk(
  "household/completeOnboarding",
  async (
    payload: {
      familyId: string;
      selfName: string;
      selfBirthDate?: string | null;
      additionalMembers?: AdditionalMemberRequest[];
    },
    { rejectWithValue },
  ) => {
    try {
      return await domusmindApi.completeOnboarding(payload.familyId, {
        selfName: payload.selfName,
        selfBirthDate: payload.selfBirthDate,
        additionalMembers: payload.additionalMembers,
      });
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to complete onboarding",
      );
    }
  },
);

export const updateHouseholdSettings = createAsyncThunk(
  "household/updateSettings",
  async (
    payload: { familyId: string } & UpdateFamilySettingsRequest,
    { rejectWithValue },
  ) => {
    try {
      return await domusmindApi.updateFamilySettings(payload.familyId, {
        name: payload.name,
        primaryLanguageCode: payload.primaryLanguageCode,
        firstDayOfWeek: payload.firstDayOfWeek,
        dateFormatPreference: payload.dateFormatPreference,
      });
    } catch (err: unknown) {
      return rejectWithValue(
        (err as { message?: string }).message ?? "Failed to update settings",
      );
    }
  },
);
