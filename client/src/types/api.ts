export type Region = {
  id: string;
  displayName: string;
  timeZoneId: string;
};

export type TransactionStatus = 'Approved' | 'Rejected';

export type SimulateTransactionRequest = {
  regionId: string;
  /** ISO-8601 instant with offset, e.g. "2026-06-11T14:24:00+03:00". */
  submittedAt: string;
};

export type TransactionSimulationResponse = {
  id: string;
  regionId: string;
  regionName: string;
  timeZoneId: string;
  localDate: string; // yyyy-MM-dd
  localTime: string; // HH:mm
  status: TransactionStatus;
  decisionReason: string;
  submittedAtUtc: string;
  createdAtUtc: string;
};

export type ApprovedTransaction = {
  id: string;
  regionName: string;
  timeZoneId: string;
  localDate: string;
  localTime: string;
  createdAtUtc: string;
};

export type AuthResponse = {
  token: string;
  username: string;
};
