import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';

import { useLocalUriCache } from './store';

const API_URL = process.env.EXPO_PUBLIC_API_URL;

if (!API_URL) {
  throw new Error('EXPO_PUBLIC_API_URL is not set');
}

type UploadUrlResponse = {
  uploadUrl: string;
  objectKey: string;
  expiresAt: string;
};

export type Photo = {
  id: string;
  objectKey: string;
  contentType: string;
  capturedAt: string;
  createdAt: string;
  url: string;
  urlExpiresAt: string;
};

export type SeverityTrend = 'Improved' | 'Similar' | 'Worsened';

export type Observation = {
  area: string;
  change: string;
  notes: string;
};

export type ComparisonResult = {
  overallSummary: string;
  observations: Observation[];
  severityTrend: SeverityTrend;
  generatedAt: string;
};

const photosQueryKey = ['photos'] as const;

const requestUploadUrl = async (contentType: string): Promise<UploadUrlResponse> => {
  const response = await fetch(`${API_URL}/api/v1/photos/upload-url`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ contentType }),
  });

  if (!response.ok) {
    throw new Error(`upload-url failed: ${response.status}`);
  }

  return response.json();
};

const putPhotoToS3 = async (
  uploadUrl: string,
  fileUri: string,
  contentType: string
): Promise<void> => {
  const fileResponse = await fetch(fileUri);
  const blob = await fileResponse.blob();

  const putResponse = await fetch(uploadUrl, {
    method: 'PUT',
    headers: { 'Content-Type': contentType },
    body: blob,
  });

  if (!putResponse.ok) {
    throw new Error(`S3 upload failed: ${putResponse.status}`);
  }
};

const confirmPhoto = async (
  objectKey: string,
  contentType: string
): Promise<Photo> => {
  const response = await fetch(`${API_URL}/api/v1/photos`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      objectKey,
      contentType,
      capturedAt: new Date().toISOString(),
    }),
  });

  if (!response.ok) {
    throw new Error(`confirm failed: ${response.status}`);
  }

  return response.json();
};

const fetchPhotos = async (): Promise<Photo[]> => {
  const response = await fetch(`${API_URL}/api/v1/photos`);

  if (!response.ok) {
    throw new Error(`list failed: ${response.status}`);
  }

  return response.json();
};

const THIRTEEN_MINUTES_MS = 13 * 60 * 1000;

export const usePhotos = () =>
  useQuery({
    queryKey: photosQueryKey,
    queryFn: fetchPhotos,
    staleTime: THIRTEEN_MINUTES_MS,
  });

type UploadPhotoArgs = {
  localUri: string;
  contentType: string;
};

type ComparePhotosArgs = {
  beforeId: string;
  afterId: string;
};

const comparePhotos = async ({
  beforeId,
  afterId,
}: ComparePhotosArgs): Promise<ComparisonResult> => {
  const response = await fetch(`${API_URL}/api/v1/photos/compare`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ beforeId, afterId }),
  });

  if (!response.ok) {
    throw new Error(`compare failed: ${response.status}`);
  }

  return response.json();
};

export const useComparePhotos = () =>
  useMutation({ mutationFn: comparePhotos });

export const useUploadPhoto = () => {
  const setLocalUri = useLocalUriCache((s) => s.setLocalUri);
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ localUri, contentType }: UploadPhotoArgs) => {
      const { uploadUrl, objectKey } = await requestUploadUrl(contentType);
      await putPhotoToS3(uploadUrl, localUri, contentType);
      const photo = await confirmPhoto(objectKey, contentType);
      setLocalUri(photo.objectKey, localUri);
      return photo;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: photosQueryKey });
    },
  });
};
