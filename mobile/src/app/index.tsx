import { FlatList, Image, Pressable, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useRouter } from 'expo-router';
import { Camera } from 'lucide-react-native';

import { usePhotosStore } from '@/features/photos/store';

const dateLabel = new Intl.DateTimeFormat('en-GB', {
  weekday: 'long',
  day: 'numeric',
  month: 'long',
}).format(new Date());

const HomeScreen = () => {
  const router = useRouter();
  const photos = usePhotosStore((s) => s.photos);

  return (
    <SafeAreaView className="flex-1 bg-sage-200">
      <View className="flex-1 px-6 pt-8">
        <Text className="text-sage-700 text-sm uppercase tracking-widest">
          {dateLabel}
        </Text>
        <Text className="text-5xl text-sage-900 font-display mt-1">
          Dermalog
        </Text>
        <Text className="text-base text-sage-800 mt-2 max-w-xs leading-snug">
          Track your skin over time. Capture, log, and see what changes.
        </Text>

        {photos.length === 0 ? (
          <View className="flex-1 items-center justify-center">
            <View className="bg-cream rounded-3xl px-8 py-10 items-center max-w-xs">
              <View className="w-14 h-14 rounded-full bg-sage-100 items-center justify-center mb-4">
                <Camera size={28} color="#3A453E" strokeWidth={1.5} />
              </View>
              <Text className="text-sage-900 text-lg font-display-medium">
                No entries yet
              </Text>
              <Text className="text-sage-700 text-center text-sm mt-1">
                Tap Capture to start tracking.
              </Text>
            </View>
          </View>
        ) : (
          <View className="flex-1">
            <Text className="text-sage-800 text-xs uppercase tracking-widest mt-6 mb-3">
              Recent entries
            </Text>
            <FlatList
              data={photos}
              keyExtractor={(p) => p.id}
              numColumns={2}
              columnWrapperClassName="gap-3"
              contentContainerClassName="gap-3 pb-4"
              renderItem={({ item }) => (
                <View className="flex-1 aspect-square rounded-2xl overflow-hidden bg-cream">
                  <Image source={{ uri: item.uri }} className="w-full h-full" />
                </View>
              )}
            />
          </View>
        )}

        <Pressable
          onPress={() => router.push('/capture')}
          className="bg-sage-900 rounded-2xl py-4 px-6 mb-4 active:opacity-80"
        >
          <Text className="text-cream text-center text-lg font-display-medium">
            Capture
          </Text>
        </Pressable>
      </View>
    </SafeAreaView>
  );
};

export default HomeScreen;
