import { Text, View } from 'react-native';

import { type SeverityTrend } from '@/features/photos/api';

const trendStyles: Record<SeverityTrend, { bg: string; text: string }> = {
  Improved: { bg: 'bg-mint', text: 'text-sage-900' },
  Similar: { bg: 'bg-peach', text: 'text-sage-900' },
  Worsened: { bg: 'bg-coral', text: 'text-cream' },
};

export const TrendChip = ({ trend }: { trend: SeverityTrend }) => {
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
