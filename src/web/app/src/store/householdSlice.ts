import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import {
  domusmindApi,
  type FamilyResponse,
  type FamilyMemberResponse,
  type AdditionalMemberRequest,
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
    const familyId = getStoredFamilyId();
    if (!familyId) return null;
    try {
      const family = await domusmindApi.getFamily(familyId);
      const members = await domusmindApi.getMembers(familyId);
      return { family, members };
    } catch {
      clearStoredFamilyId();
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
    { familyId, name, role }: { familyId: string; name: string; role: string },
    { rejectWithValue },
  ) => {
    try {
      return await domusmindApi.addMember(familyId, { name, role });
    } catch (err: unknown) {
      return rejectWithValue((err as { message?: string }).message ?? "Failed to add person");
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
        state.members.push(action.payload);
      })
      .addCase(completeOnboarding.fulfilled, (state, action) => {
        state.members = action.payload.members.map((m) => ({
          memberId: m.memberId,
          familyId: action.payload.familyId,
          name: m.name,
          role: m.role,
          joinedAtUtc: m.joinedAtUtc,
        }));
        state.bootstrapStatus = "ready";
        state.error = null;
      });
  },
});

export const { resetHousehold } = householdSlice.actions;
export default householdSlice.reducer;
