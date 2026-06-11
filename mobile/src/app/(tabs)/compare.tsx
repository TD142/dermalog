import { useState } from 'react';
import {
  ActivityIndicator,
  FlatList,
  Image,
  Pressable,
  ScrollView,
  Text,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Camera, ImageIcon } from 'lucide-react-native';

import {
  type Comparison,
  type Photo,
  useComparePhotos,
  usePhotos,
} from '@/features/photos/api';
import { TrendChip } from '@/features/photos/components/trend-chip';
import { useLocalUriCache } from '@/features/photos/store';

const ResultCard = ({ result }: { result: Comparison }) => (
  <View className="bg-cream rounded-3xl p-5 mt-4">
    <TrendChip trend={result.severityTrend} />
    <Text className="text-sage-900 text-base mt-3 leading-snug">
      {result.overallSummary}
    </Text>
    {result.observations.length > 0 && (
      <View className="mt-4 gap-3">
        {result.observations.map((o, i) => (
          <View key={i}>
            <Text className="text-sage-900 font-display-medium text-sm">
              {o.area} — {o.change}
            </Text>
            {o.notes ? (
              <Text className="text-sage-700 text-sm mt-0.5">{o.notes}</Text>
            ) : null}
          </View>
        ))}
      </View>
    )}
  </View>
);

const PhotoSlot = ({
  label,
  photo,
  localUri,
  onClear,
}: {
  label: string;
  photo: Photo | undefined;
  localUri: string | undefined;
  onClear: () => void;
}) => {
  const uri = localUri ?? photo?.url;
  return (
    <View className="flex-1">
      <Text className="text-sage-700 text-xs uppercase tracking-widest mb-1">
        {label}
      </Text>
      <Pressable
        onPress={onClear}
        className="aspect-square rounded-2xl overflow-hidden bg-cream items-center justify-center"
      >
        {uri ? (
          <Image source={{ uri }} className="w-full h-full" />
        ) : (
          <ImageIcon size={28} color="#8FA395" strokeWidth={1.5} />
        )}
      </Pressable>
    </View>
  );
};

const NeedMorePhotos = () => (
  <View className="flex-1 items-center justify-center">
    <View className="bg-cream rounded-3xl px-8 py-10 items-center max-w-xs">
      <View className="w-14 h-14 rounded-full bg-sage-100 items-center justify-center mb-4">
        <Camera size={28} color="#3A453E" strokeWidth={1.5} />
      </View>
      <Text className="text-sage-900 text-lg font-display-medium text-center">
        Two photos needed
      </Text>
      <Text className="text-sage-700 text-center text-sm mt-1">
        Capture at least two photos of the same area, then come back to compare
        them.
      </Text>
    </View>
  </View>
);

const CompareScreen = () => {
  const { data: photos = [] } = usePhotos();
  const compare = useComparePhotos();
  const getLocalUri = useLocalUriCache((s) => s.getLocalUri);

  const [beforeId, setBeforeId] = useState<string | undefined>();
  const [afterId, setAfterId] = useState<string | undefined>();

  const before = photos.find((p) => p.id === beforeId);
  const after = photos.find((p) => p.id === afterId);
  const canCompare = beforeId && afterId && beforeId !== afterId;

  const onSelectPhoto = (id: string) => {
    if (id === beforeId) {
      setBeforeId(undefined);
      return;
    }
    if (id === afterId) {
      setAfterId(undefined);
      return;
    }
    if (!beforeId) {
      setBeforeId(id);
    } else if (!afterId) {
      setAfterId(id);
    } else {
      setBeforeId(afterId);
      setAfterId(id);
    }
  };

  const onCompare = () => {
    if (!beforeId || !afterId) return;
    compare.mutate({ beforeId, afterId });
  };

  return (
    <SafeAreaView edges={['top']} className="flex-1 bg-sage-200">
      <ScrollView contentContainerClassName="px-6 pt-8 pb-12 grow">
        <Text className="text-4xl text-sage-900 font-display">Compare</Text>
        <Text className="text-base text-sage-800 mt-1">
          Pick two photos to see what&apos;s changed.
        </Text>

        {photos.length < 2 ? (
          <NeedMorePhotos />
        ) : (
          <>
            <View className="flex-row gap-3 mt-6">
              <PhotoSlot
                label="Before"
                photo={before}
                localUri={before ? getLocalUri(before.objectKey) : undefined}
                onClear={() => setBeforeId(undefined)}
              />
              <PhotoSlot
                label="After"
                photo={after}
                localUri={after ? getLocalUri(after.objectKey) : undefined}
                onClear={() => setAfterId(undefined)}
              />
            </View>

            <Text className="text-sage-800 text-xs uppercase tracking-widest mt-6 mb-2">
              Your photos
            </Text>
            <FlatList
              data={photos}
              keyExtractor={(p) => p.id}
              horizontal
              showsHorizontalScrollIndicator={false}
              contentContainerClassName="gap-2"
              renderItem={({ item }) => {
                const localUri = getLocalUri(item.objectKey);
                const uri = localUri ?? item.url;
                const isSelected = item.id === beforeId || item.id === afterId;
                return (
                  <Pressable
                    onPress={() => onSelectPhoto(item.id)}
                    className={`w-20 h-20 rounded-xl overflow-hidden ${
                      isSelected ? 'border-2 border-sage-900' : ''
                    }`}
                  >
                    {uri ? (
                      <Image source={{ uri }} className="w-full h-full" />
                    ) : (
                      <View className="w-full h-full bg-cream" />
                    )}
                  </Pressable>
                );
              }}
            />

            <Pressable
              onPress={onCompare}
              disabled={!canCompare || compare.isPending}
              className="bg-sage-900 rounded-2xl py-4 px-6 mt-6 active:opacity-80 disabled:opacity-40"
            >
              <Text className="text-cream text-center text-lg font-display-medium">
                {compare.isPending ? 'Comparing…' : 'Compare'}
              </Text>
            </Pressable>

            {compare.isPending && (
              <View className="items-center mt-6">
                <ActivityIndicator color="#3A453E" />
                <Text className="text-sage-700 text-sm mt-2">
                  Reading your photos…
                </Text>
              </View>
            )}

            {compare.isError && (
              <View className="bg-coral/20 rounded-2xl p-4 mt-4">
                <Text className="text-sage-900">
                  Couldn&apos;t compare those photos. Try again.
                </Text>
              </View>
            )}

            {compare.data && <ResultCard result={compare.data} />}
          </>
        )}
      </ScrollView>
    </SafeAreaView>
  );
};

export default CompareScreen;
