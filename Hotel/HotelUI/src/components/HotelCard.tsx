import React from 'react';
import { 
  makeStyles, 
  shorthands, 
  tokens, 
  Card, 
  CardPreview, 
  Text, 
  Caption1, 
  Button,
  Subtitle1,
  Badge
} from '@fluentui/react-components';
import { Star24Filled, Location24Regular, ArrowRight24Regular } from '@fluentui/react-icons';

const useStyles = makeStyles({
  card: {
    width: '100%',
    maxWidth: '400px',
    backgroundColor: tokens.colorNeutralBackground1,
    ...shorthands.borderRadius(tokens.borderRadiusLarge),
    ...shorthands.border('1px', 'solid', tokens.colorNeutralStroke2),
    boxShadow: tokens.shadow16, // Sombra visível por padrão
    transition: 'transform 0.2s ease-in-out, box-shadow 0.2s ease-in-out',
    ':hover': {
      transform: 'translateY(-4px)',
      boxShadow: tokens.shadow28, // Sombra mais forte no hover
      ...shorthands.borderColor(tokens.colorBrandStroke1), // Destaque na borda
    },
  },
  preview: {
    height: '200px',
    overflow: 'hidden',
  },
  image: {
    width: '100%',
    height: '100%',
    objectFit: 'cover',
  },
  content: {
    ...shorthands.padding('16px'),
  },
  footer: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginTop: '12px',
  },
  location: {
    display: 'flex',
    alignItems: 'center',
    gap: '4px',
    color: tokens.colorNeutralForeground2,
  },
  goldStar: {
    color: '#C5A059', // Gold fixed color
  }
});

interface HotelCardProps {
  name: string;
  location: string;
  stars: number;
  description: string;
  imageUrl: string;
  onViewRooms: () => void;
}

export const HotelCard: React.FC<HotelCardProps> = ({ 
  name, 
  location, 
  stars, 
  description, 
  imageUrl,
  onViewRooms 
}) => {
  const styles = useStyles();

  return (
    <Card className={styles.card} appearance="subtle">
      <CardPreview className={styles.preview}>
        <img src={imageUrl} alt={name} className={styles.image} />
      </CardPreview>
      
      <div className={styles.content}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
          <Subtitle1>{name}</Subtitle1>
          <div style={{ display: 'flex', alignItems: 'center' }}>
            <Star24Filled className={styles.goldStar} />
            <Text weight="semibold">{stars}</Text>
          </div>
        </div>

        <Caption1 className={styles.location}>
          <Location24Regular fontSize={16} />
          {location}
        </Caption1>

        <Text block size={200} style={{ marginTop: '12px', color: tokens.colorNeutralForeground3, height: '40px', overflow: 'hidden' }}>
          {description}
        </Text>

        <div className={styles.footer}>
          <Badge appearance="outline" color="informative">Luxury Stay</Badge>
          <Button 
            appearance="transparent" 
            icon={<ArrowRight24Regular />} 
            iconPosition="after"
            onClick={onViewRooms}
          >
            Ver Quartos
          </Button>
        </div>
      </div>
    </Card>
  );
};
