import { Alert, Image, Pressable, ScrollView, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { ArrowLeft, ImageIcon, Trash2 } from 'lucide-react-native';

import { useDeletePhoto, usePhotos } from '@/features/photos/api';
import { useLocalUriCache } from '@/features/photos/store';

const formatDate = (iso: string) =>
  new Intl.DateTimeFormat('en-GB', {
    weekday: 'long',
    day: 'numeric',
    month: 'long',
    year: 'numeric',
  }).format(new Date(iso));

const PhotoDetailScreen = () => {
  const router = useRouter();
  const { id } = useLocalSearchParams<{ id: string }>();
  const { data: photos = [] } = usePhotos();
  const getLocalUri = useLocalUriCache((s) => s.getLocalUri);
  const remove = useDeletePhoto();

  const photo = photos.find((p) => p.id === id);
  const uri = photo ? (getLocalUri(photo.objectKey) ?? photo.url) : undefined;

  const onDelete = () =>
    Alert.alert('Delete photo?', 'This permanently removes the photo.', [
      { text: 'Cancel', style: 'cancel' },
      {
        text: 'Delete',
        style: 'destructive',
        onPress: () =>
          remove.mutate(id, {
            onSuccess: () => router.back(),
            onError: () =>
              Alert.alert(
                "Couldn't delete",
                'If this photo is part of a comparison, delete that comparison first.'
              ),
          }),
      },
    ]);

  return (
    <SafeAreaView className="flex-1 bg-sage-200">
      <ScrollView contentContainerClassName="px-6 pt-4 pb-12">
        <Pressable
          onPress={() => router.back()}
          className="flex-row items-center mb-4"
        >
          <ArrowLeft size={20} color="#3A453E" strokeWidth={1.5} />
          <Text className="text-sage-900 ml-1">Back</Text>
        </Pressable>

        {photo ? (
          <>
            <View className="rounded-3xl overflow-hidden bg-cream aspect-square items-center justify-center">
              {uri ? (
                <Image source={{ uri }} className="w-full h-full" />
              ) : (
                <ImageIcon size={32} color="#8FA395" strokeWidth={1.5} />
              )}
            </View>

            <Text className="text-sage-700 text-xs uppercase tracking-widest mt-4">
              Captured
            </Text>
            <Text className="text-sage-900 text-lg font-display-medium mt-0.5">
              {formatDate(photo.capturedAt)}
            </Text>

            <Pressable
              onPress={onDelete}
              disabled={remove.isPending}
              className="flex-row items-center justify-center border border-coral rounded-2xl py-4 px-6 mt-8 active:opacity-80 disabled:opacity-50"
            >
              <Trash2 size={18} color="#D08573" strokeWidth={1.5} />
              <Text className="text-coral text-center text-lg font-display-medium ml-2">
                {remove.isPending ? 'Deleting…' : 'Delete photo'}
              </Text>
            </Pressable>
          </>
        ) : (
          <Text className="text-sage-800 mt-8">Photo not found.</Text>
        )}
      </ScrollView>
    </SafeAreaView>
  );
};

export default PhotoDetailScreen;
