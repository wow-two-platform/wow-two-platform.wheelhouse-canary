/** Represents one recorded ping. */
export interface Ping {
  readonly id: string;
  readonly note: string;
  readonly createdAt: Date;
}
