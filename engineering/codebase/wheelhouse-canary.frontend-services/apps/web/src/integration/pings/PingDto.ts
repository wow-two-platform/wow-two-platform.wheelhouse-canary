/** Represents one ping on the wire, inside the `{ data }` envelope of GET and POST /api/pings. */
export interface PingDto {
  readonly id: string;
  readonly note: string;
  readonly createdAt: string;
}
