import { createSlice } from "@reduxjs/toolkit";
import type { FamilyResponse, FamilyMemberResponse } from "../api/domusmindApi";
import {
  bootstrapHousehold,
  createFamily,
  fetchMembers,
  addMember,
  linkMemberAccount,
  updateMemberProfile,
  updateMember,
  completeOnboarding,
  updateHouseholdSettings,
} from "./householdThunks";

export {
  getStoredFamilyId,
  storeFamilyId,
  clearStoredFamilyId,
  bootstrapHousehold,
  createFamily,
  fetchMembers,
  addMember,
  inviteMember,
  linkMemberAccount,
  provisionMemberAccess,
  regeneratePassword,
  disableMemberAccess,
  enableMemberAccess,
  updateMemberProfile,
  updateMember,
  completeOnboarding,
  updateHouseholdSettings,
} from "./householdThunks";

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
      .addCase(addMember.fulfilled, (_state, _action) => {
        // Re-fetch to get full server-computed fields; fetchMembers dispatched by the thunk.
      })
      .addCase(completeOnboarding.fulfilled, (state, action) => {
        state.members = action.payload.members.map((m) => ({
          memberId: m.memberId,
          familyId: action.payload.familyId,
          name: m.name,
          preferredName: null,
          role: m.role,
          isManager: m.isManager,
          birthDate: m.birthDate,
          joinedAtUtc: m.joinedAtUtc,
          authUserId: null,
          accessStatus: "NoAccess" as const,
          linkedEmail: null,
          isCurrentUser: false,
          hasAccount: false,
          canGrantAccess: false,
          canEdit: false,
          avatarInitial: m.name?.[0]?.toUpperCase() ?? "?",
        }));
        state.bootstrapStatus = "ready";
        state.error = null;
      })
      .addCase(updateMember.fulfilled, (state, action) => {
        const idx = state.members.findIndex((m) => m.memberId === action.payload.memberId);
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
      .addCase(updateMemberProfile.fulfilled, (state, action) => {
        const idx = state.members.findIndex((m) => m.memberId === action.payload.memberId);
        if (idx !== -1) {
          state.members[idx] = {
            ...state.members[idx],
            preferredName: action.payload.preferredName,
          };
        }
      })
      .addCase(linkMemberAccount.fulfilled, (state, action) => {
        const idx = state.members.findIndex((m) => m.memberId === action.payload.memberId);
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
