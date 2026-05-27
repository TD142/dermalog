import { useMutation } from '@tanstack/react-query';

import { usePhotosStore } from './store';

const API_URL = process.env.EXPO_PUBLIC_API_URL;

if (!API_URL) {
  throw new Error('EXPO_PUBLIC_API_URL is not set');
}

type UploadUrlResponse = {
  uploadUrl: string;
  objectKey: string;
  expiresAt: string;
};

const requestUploadUrl = async (
  contentType: string
): Promise<UploadUrlResponse> => {
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

type UploadPhotoArgs = {
  localUri: string;
  contentType: string;
};

export const useUploadPhoto = () => {
  const addPhoto = usePhotosStore((s) => s.addPhoto);

  return useMutation({
    mutationFn: async ({ localUri, contentType }: UploadPhotoArgs) => {
      const { uploadUrl, objectKey } = await requestUploadUrl(contentType);
      await putPhotoToS3(uploadUrl, localUri, contentType);
      addPhoto(localUri, objectKey);
      return { objectKey };
    },
  });
};
