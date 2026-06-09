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
import { useRouter } from 'expo-router';
import { ArrowLeft, ImageIcon } from 'lucide-react-native';

import {
  type ComparisonResult,
  type Photo,
  type SeverityTrend,
  useComparePhotos,
  usePhotos,
} from '@/features/photos/api';
import { useLocalUriCache } from '@/features/photos/store';

const trendStyles: Record<SeverityTrend, { bg: string; text: string }> = {
  Improved: { bg: 'bg-mint', text: 'text-sage-900' },
  Similar: { bg: 'bg-peach', text: 'text-sage-900' },
  Worsened: { bg: 'bg-coral', text: 'text-cream' },
};

const TrendChip = ({ trend }: { trend: SeverityTrend }) => {
  const style = trendStyles[trend];
  return (
    <View className={`self-start px-3 py-1 rounded-full ${style.bg}`}>
      <Text
        className={`text-xs uppercase tracking-widest font-display-medium ${style.text}`}
      >
        {trend}
      </Text>
    </View>
  );
};

const ResultCard = ({ result }: { result: ComparisonResult }) => (
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

const CompareScreen = () => {
  const router = useRouter();
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
    <SafeAreaView className="flex-1 bg-sage-200">
      <ScrollView contentContainerClassName="px-6 pt-4 pb-12">
        <Pressable
          onPress={() => router.back()}
          className="flex-row items-center mb-2"
        >
          <ArrowLeft size={20} color="#3A453E" strokeWidth={1.5} />
          <Text className="text-sage-900 ml-1">Back</Text>
        </Pressable>

        <Text className="text-4xl text-sage-900 font-display mt-2">Compare</Text>
        <Text className="text-base text-sage-800 mt-1">
          Pick two photos to see what&apos;s changed.
        </Text>

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

        {photos.length > 0 && (
          <>
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
          </>
        )}

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
      </ScrollView>
    </SafeAreaView>
  );
};

export default CompareScreen;
