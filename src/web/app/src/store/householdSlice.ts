import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import {
  domusmindApi,
  type FamilyResponse,
  type FamilyMemberResponse,
  type AdditionalMemberRequest,
  type UpdateFamilySettingsRequest,
  type InviteMemberRequest,
  type LinkMemberAccountRequest,
  type ProvisionMemberAccessRequest,
  type UpdateMemberRequest,
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

interface HouseholdState {
  family: FamilyResponse | null;
  members: FamilyMemberResponse[];
  bootstrapStatus: "idle" | "loading" | "ready" | "needsOnboarding" | "error";
  error: string | null;
}

const initialState: HouseholdState = {
  family: null,
  members: [],
  bootstrapStatus: "idle",
  error: null,
};

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

export const addMember = createAsyncThunk(
  "household/addMember",
  async (
    { familyId, name, role, birthDate, isManager }: { familyId: string; name: string; role: string; birthDate?: string | null; isManager?: boolean },
    { rejectWithValue },
  ) => {
    try {
      return await domusmindApi.addMember(familyId, { name, role, birthDate, isManager });
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
      // Refresh member list to pick up full member data
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
      // Refresh member list to get updated authUserId
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

const householdSlice = createSlice({
  name: "household",
  initialState,
  reducers: {
    resetHousehold(state) {
      state.family = null;
      state.members = [];
      state.bootstrapStatus = "needsOnboarding";
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(bootstrapHousehold.pending, (state) => {
        state.bootstrapStatus = "loading";
      })
      .addCase(bootstrapHousehold.fulfilled, (state, action) => {
        if (action.payload) {
          state.family = action.payload.family;
          state.members = action.payload.members;
          state.bootstrapStatus = "ready";
        } else {
          state.bootstrapStatus = "needsOnboarding";
        }
      })
      .addCase(bootstrapHousehold.rejected, (state) => {
        state.bootstrapStatus = "needsOnboarding";
      })
      .addCase(createFamily.fulfilled, (state, action) => {
        state.family = action.payload;
        state.members = [];
        state.bootstrapStatus = "needsOnboarding";
        state.error = null;
      })
      .addCase(createFamily.rejected, (state, action) => {
        state.error = action.payload as string;
      })
      .addCase(fetchMembers.fulfilled, (state, action) => {
        state.members = action.payload;
      })
      .addCase(addMember.fulfilled, (state, action) => {
        state.members.push({
          ...action.payload,
          isManager: false,
          birthDate: null,
          authUserId: null,
          accessStatus: "None",
          linkedEmail: null,
        });
      })
      .addCase(completeOnboarding.fulfilled, (state, action) => {
        state.members = action.payload.members.map((m) => ({
          memberId: m.memberId,
          familyId: action.payload.familyId,
          name: m.name,
          role: m.role,
          isManager: m.isManager,
          birthDate: m.birthDate,
          joinedAtUtc: m.joinedAtUtc,
          authUserId: null,
          accessStatus: "None" as const,
          linkedEmail: null,
        }));
        state.bootstrapStatus = "ready";
        state.error = null;
      })
      .addCase(updateMember.fulfilled, (state, action) => {
        const idx = state.members.findIndex(
          (m) => m.memberId === action.payload.memberId,
        );
        if (idx !== -1) {
          state.members[idx] = {
            ...state.members[idx],
            name: action.payload.name,
            role: action.payload.role,
            isManager: action.payload.isManager,
            birthDate: action.payload.birthDate,
          };
        }
      })
      .addCase(linkMemberAccount.fulfilled, (state, action) => {
        const idx = state.members.findIndex(
          (m) => m.memberId === action.payload.memberId,
        );
        if (idx !== -1) {
          state.members[idx] = {
            ...state.members[idx],
            authUserId: action.payload.authUserId,
          };
        }
      })
      .addCase(updateHouseholdSettings.fulfilled, (state, action) => {
        if (state.family) {
          state.family.name = action.payload.name;
          state.family.primaryLanguageCode = action.payload.primaryLanguageCode;
          state.family.firstDayOfWeek = action.payload.firstDayOfWeek;
          state.family.dateFormatPreference = action.payload.dateFormatPreference;
        }
      });
  },
});

export const { resetHousehold } = householdSlice.actions;
export default householdSlice.reducer;
