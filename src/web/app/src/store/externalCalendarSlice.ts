import { createAsyncThunk, createSlice } from "@reduxjs/toolkit";
import {
  externalCalendarApi,
  type ExternalCalendarConnectionSummary,
  type ExternalCalendarConnectionDetail,
  type ConnectOutlookAccountRequest,
  type ConfigureConnectionRequest,
} from "../api/externalCalendarApi";

interface ExternalCalendarState {
  connections: ExternalCalendarConnectionSummary[];
  detail: ExternalCalendarConnectionDetail | null;
  status: "idle" | "loading" | "error";
  error: string | null;
}

const initialState: ExternalCalendarState = {
  connections: [],
  detail: null,
  status: "idle",
  error: null,
};

export const fetchConnections = createAsyncThunk(
  "externalCalendar/fetchConnections",
  async ({ familyId, memberId }: { familyId: string; memberId: string }) =>
    externalCalendarApi.listConnections(familyId, memberId),
);

export const fetchConnectionDetail = createAsyncThunk(
  "externalCalendar/fetchDetail",
  async ({
    familyId,
    memberId,
    connectionId,
  }: {
    familyId: string;
    memberId: string;
    connectionId: string;
  }) => externalCalendarApi.getConnectionDetail(familyId, memberId, connectionId),
);

export const connectOutlook = createAsyncThunk(
  "externalCalendar/connectOutlook",
  async ({
    familyId,
    memberId,
    body,
  }: {
    familyId: string;
    memberId: string;
    body: ConnectOutlookAccountRequest;
  }) => externalCalendarApi.connectOutlook(familyId, memberId, body),
);

export const configureConnection = createAsyncThunk(
  "externalCalendar/configure",
  async ({
    familyId,
    memberId,
    connectionId,
    body,
  }: {
    familyId: string;
    memberId: string;
    connectionId: string;
    body: ConfigureConnectionRequest;
  }) => externalCalendarApi.configureConnection(familyId, memberId, connectionId, body),
);

export const syncConnection = createAsyncThunk(
  "externalCalendar/sync",
  async ({
    familyId,
    memberId,
    connectionId,
  }: {
    familyId: string;
    memberId: string;
    connectionId: string;
  }) => externalCalendarApi.syncConnection(familyId, memberId, connectionId),
);

export const disconnectConnection = createAsyncThunk(
  "externalCalendar/disconnect",
  async ({
    familyId,
    memberId,
    connectionId,
  }: {
    familyId: string;
    memberId: string;
    connectionId: string;
  }) => {
    await externalCalendarApi.disconnectConnection(familyId, memberId, connectionId);
    return connectionId;
  },
);

const externalCalendarSlice = createSlice({
  name: "externalCalendar",
  initialState,
  reducers: {
    clearDetail(state) {
      state.detail = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchConnections.pending, (state) => {
        state.status = "loading";
        state.error = null;
      })
      .addCase(fetchConnections.fulfilled, (state, action) => {
        state.status = "idle";
        state.connections = action.payload;
      })
      .addCase(fetchConnections.rejected, (state, action) => {
        state.status = "error";
        state.error = action.error.message ?? "Failed to load connections";
      })
      .addCase(fetchConnectionDetail.fulfilled, (state, action) => {
        state.detail = action.payload;
      })
      .addCase(fetchConnectionDetail.rejected, (state, action) => {
        state.error = action.error.message ?? "Failed to load connection detail";
      })
      .addCase(connectOutlook.fulfilled, (state) => {
        state.status = "idle";
      })
      .addCase(disconnectConnection.fulfilled, (state, action) => {
        state.connections = state.connections.filter((c) => c.connectionId !== action.payload);
        if (state.detail?.connectionId === action.payload) state.detail = null;
      });
  },
});

export const { clearDetail } = externalCalendarSlice.actions;
export default externalCalendarSlice.reducer;
