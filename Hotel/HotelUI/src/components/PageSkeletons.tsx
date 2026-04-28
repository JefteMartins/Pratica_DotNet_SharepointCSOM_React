import React from 'react';
import {
  Card,
  Skeleton,
  SkeletonItem,
  Table,
  TableBody,
  TableCell,
  TableHeader,
  TableHeaderCell,
  TableRow,
  makeStyles,
  shorthands,
  tokens,
} from '@fluentui/react-components';

const useStyles = makeStyles({
  dashboardGrid: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fill, minmax(240px, 1fr))',
    gap: '24px',
  },
  dashboardCard: {
    ...shorthands.padding('24px'),
    display: 'flex',
    flexDirection: 'column',
    gap: '12px',
    backgroundColor: tokens.colorNeutralBackground1,
    boxShadow: tokens.shadow16,
    ...shorthands.borderRadius(tokens.borderRadiusLarge),
    ...shorthands.border('1px', 'solid', tokens.colorNeutralStroke2),
  },
  hotelGrid: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fill, minmax(320px, 1fr))',
    gap: '32px',
    marginTop: '20px',
  },
  hotelCard: {
    width: '100%',
    maxWidth: '400px',
    backgroundColor: tokens.colorNeutralBackground1,
    ...shorthands.borderRadius(tokens.borderRadiusLarge),
    ...shorthands.border('1px', 'solid', tokens.colorNeutralStroke2),
    boxShadow: tokens.shadow16,
    overflow: 'hidden',
  },
  hotelContent: {
    ...shorthands.padding('16px'),
    display: 'flex',
    flexDirection: 'column',
    gap: '12px',
  },
  hotelFooter: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginTop: '8px',
  },
  roomsFilterBar: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))',
    gap: '20px',
    backgroundColor: tokens.colorNeutralBackground2,
    ...shorthands.padding('24px'),
    borderRadius: tokens.borderRadiusLarge,
    boxShadow: tokens.shadow4,
  },
  roomsGrid: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))',
    gap: '24px',
  },
  roomCard: {
    display: 'flex',
    flexDirection: 'row',
    backgroundColor: tokens.colorNeutralBackground1,
    ...shorthands.border('1px', 'solid', tokens.colorNeutralStroke2),
    ...shorthands.borderRadius(tokens.borderRadiusLarge),
    overflow: 'hidden',
  },
  roomStatusBar: {
    width: '8px',
    backgroundColor: tokens.colorNeutralStrokeAccessible,
  },
  roomContent: {
    ...shorthands.padding('16px'),
    flexGrow: 1,
    display: 'flex',
    flexDirection: 'column',
    gap: '10px',
  },
  bookingsContainer: {
    backgroundColor: tokens.colorNeutralBackground1,
    boxShadow: tokens.shadow16,
    ...shorthands.borderRadius(tokens.borderRadiusLarge),
    overflow: 'hidden',
    ...shorthands.border('1px', 'solid', tokens.colorNeutralStroke2),
  },
  bookingHeaderCell: {
    fontWeight: 'bold',
    backgroundColor: tokens.colorNeutralBackground2,
  },
});

export const DashboardSkeleton: React.FC = () => {
  const styles = useStyles();

  return (
    <div className={styles.dashboardGrid}>
      {Array.from({ length: 4 }).map((_, index) => (
        <Card key={index} className={styles.dashboardCard} appearance="subtle">
          <Skeleton>
            <SkeletonItem shape="circle" size={40} />
            <SkeletonItem size={16} style={{ width: '55%' }} />
            <SkeletonItem size={36} style={{ width: '45%' }} />
          </Skeleton>
        </Card>
      ))}
    </div>
  );
};

export const HotelGridSkeleton: React.FC = () => {
  const styles = useStyles();

  return (
    <div className={styles.hotelGrid}>
      {Array.from({ length: 6 }).map((_, index) => (
        <Card key={index} className={styles.hotelCard} appearance="subtle">
          <Skeleton>
            <SkeletonItem style={{ height: '200px', width: '100%' }} />
          </Skeleton>
          <div className={styles.hotelContent}>
            <Skeleton>
              <SkeletonItem size={24} style={{ width: '65%' }} />
              <SkeletonItem size={16} style={{ width: '40%' }} />
              <SkeletonItem size={16} style={{ width: '100%' }} />
              <SkeletonItem size={16} style={{ width: '85%' }} />
            </Skeleton>
            <div className={styles.hotelFooter}>
              <Skeleton>
                <SkeletonItem size={24} style={{ width: '96px' }} />
              </Skeleton>
              <Skeleton>
                <SkeletonItem size={28} style={{ width: '110px' }} />
              </Skeleton>
            </div>
          </div>
        </Card>
      ))}
    </div>
  );
};

export const RoomsPageSkeleton: React.FC = () => {
  const styles = useStyles();

  return (
    <>
      <div className={styles.roomsFilterBar}>
        {Array.from({ length: 5 }).map((_, index) => (
          <Skeleton key={index}>
            <SkeletonItem size={16} style={{ width: '45%', marginBottom: '8px' }} />
            <SkeletonItem size={36} style={{ width: '100%' }} />
          </Skeleton>
        ))}
      </div>

      <Skeleton>
        <SkeletonItem size={24} style={{ width: '180px', marginTop: '8px' }} />
      </Skeleton>

      <div className={styles.roomsGrid}>
        {Array.from({ length: 6 }).map((_, index) => (
          <div key={index} className={styles.roomCard}>
            <div className={styles.roomStatusBar} />
            <div className={styles.roomContent}>
              <Skeleton>
                <SkeletonItem size={22} style={{ width: '55%' }} />
                <SkeletonItem size={14} style={{ width: '25%' }} />
                <SkeletonItem size={16} style={{ width: '40%' }} />
                <SkeletonItem size={16} style={{ width: '30%' }} />
                <SkeletonItem size={16} style={{ width: '35%', marginTop: '16px' }} />
              </Skeleton>
            </div>
          </div>
        ))}
      </div>
    </>
  );
};

export const BookingsTableSkeleton: React.FC = () => {
  const styles = useStyles();

  return (
    <div className={styles.bookingsContainer}>
      <Table>
        <TableHeader>
          <TableRow>
            {Array.from({ length: 8 }).map((_, index) => (
              <TableHeaderCell key={index} className={styles.bookingHeaderCell}>
                <Skeleton>
                  <SkeletonItem size={16} style={{ width: '70%' }} />
                </Skeleton>
              </TableHeaderCell>
            ))}
          </TableRow>
        </TableHeader>
        <TableBody>
          {Array.from({ length: 5 }).map((_, rowIndex) => (
            <TableRow key={rowIndex}>
              {Array.from({ length: 8 }).map((_, cellIndex) => (
                <TableCell key={cellIndex}>
                  <Skeleton>
                    <SkeletonItem size={16} style={{ width: cellIndex === 0 ? '80px' : '100%' }} />
                  </Skeleton>
                </TableCell>
              ))}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
};
