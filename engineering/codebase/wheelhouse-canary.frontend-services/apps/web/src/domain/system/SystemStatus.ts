/** Represents the running backend: its version and the label its environment gives it. */
export interface SystemStatus {
  readonly service: string;
  readonly version: string;
  readonly label: string;
}
