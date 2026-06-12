import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';

const API_URL = process.env.EXPO_PUBLIC_API_URL;

if (!API_URL) {
  throw new Error('EXPO_PUBLIC_API_URL is not set');
}

export type JournalSeverity = 'Mild' | 'Moderate' | 'Severe';

export type JournalEntry = {
  id: string;
  text: string;
  symptoms: string[];
  triggers: string[];
  treatments: string[];
  areas: string[];
  severity: JournalSeverity;
  summary: string;
  createdAt: string;
};

const journalQueryKey = ['journal'] as const;
const FIVE_MINUTES_MS = 5 * 60 * 1000;

const fetchJournal = async (): Promise<JournalEntry[]> => {
  const response = await fetch(`${API_URL}/api/v1/journal`);

  if (!response.ok) {
    throw new Error(`journal failed: ${response.status}`);
  }

  return response.json();
};

export const useJournal = () =>
  useQuery({
    queryKey: journalQueryKey,
    queryFn: fetchJournal,
    staleTime: FIVE_MINUTES_MS,
  });

const createJournalEntry = async (text: string): Promise<JournalEntry> => {
  const response = await fetch(`${API_URL}/api/v1/journal`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ text }),
  });

  if (!response.ok) {
    throw new Error(`create journal failed: ${response.status}`);
  }

  return response.json();
};

export const useCreateJournalEntry = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: createJournalEntry,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: journalQueryKey });
    },
  });
};

const deleteJournalEntry = async (id: string): Promise<void> => {
  const response = await fetch(`${API_URL}/api/v1/journal/${id}`, {
    method: 'DELETE',
  });

  if (!response.ok) {
    throw new Error(`delete journal failed: ${response.status}`);
  }
};

export const useDeleteJournalEntry = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: deleteJournalEntry,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: journalQueryKey });
    },
  });
};
