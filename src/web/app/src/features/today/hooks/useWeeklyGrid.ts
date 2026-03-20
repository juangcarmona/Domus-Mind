import { useState, useEffect, useCallback } from "react";
import { weekApi } from "../api/weekApi";
import type { WeeklyGridResponse } from "../types";
import type { ApiError } from "../../../api/domusmindApi";

const DAY_ORDER = ["sunday", "monday", "tuesday", "wednesday", "thursday", "friday", "saturday"];

function toIsoDate(d: Date): string {
  return d.toISOString().slice(0, 10);
}

function startOfWeek(d: Date, firstDayOfWeek?: string | null): Date {
  const targetDay = DAY_ORDER.indexOf((firstDayOfWeek ?? "monday").toLowerCase());
  const safeTarget = targetDay < 0 ? 1 : targetDay; // fallback to Monday
  const day = d.getDay(); // 0 = Sunday
  let diff = day - safeTarget;
  if (diff < 0) diff += 7;
  return new Date(d.getFullYear(), d.getMonth(), d.getDate() - diff);
}

interface UseWeeklyGridResult {
  grid: WeeklyGridResponse | null;
  loading: boolean;
  error: string | null;
  weekStart: Date;
  prevWeek: () => void;
  nextWeek: () => void;
  refresh: () => void;
}

export function useWeeklyGrid(familyId: string, firstDayOfWeek?: string | null): UseWeeklyGridResult {
  const [weekStart, setWeekStart] = useState<Date>(() => startOfWeek(new Date(), firstDayOfWeek));

  // Re-anchor to correct week-start when the household setting loads or changes
  useEffect(() => {
    setWeekStart(startOfWeek(new Date(), firstDayOfWeek));
  }, [firstDayOfWeek]);
  const [grid, setGrid] = useState<WeeklyGridResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchGrid = useCallback(
    async (start: Date) => {
      if (!familyId) return;
      setLoading(true);
      setError(null);
      try {
        const data = await weekApi.getWeeklyGrid(familyId, toIsoDate(start));
        setGrid(data);
      } catch (err) {
        const apiErr = err as Partial<ApiError>;
        setError(apiErr.message ?? "Failed to load weekly grid.");
      } finally {
        setLoading(false);
      }
    },
    [familyId],
  );

  useEffect(() => {
    fetchGrid(weekStart);
  }, [fetchGrid, weekStart]);

  const prevWeek = useCallback(() => {
    setWeekStart((d) => {
      const prev = new Date(d);
      prev.setDate(prev.getDate() - 7);
      return prev;
    });
  }, []);

  const nextWeek = useCallback(() => {
    setWeekStart((d) => {
      const next = new Date(d);
      next.setDate(next.getDate() + 7);
      return next;
    });
  }, []);

  const refresh = useCallback(() => fetchGrid(weekStart), [fetchGrid, weekStart]);

  return { grid, loading, error, weekStart, prevWeek, nextWeek, refresh };
}
