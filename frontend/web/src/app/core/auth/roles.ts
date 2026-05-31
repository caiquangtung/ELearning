export const Roles = {
  Admin: 'Admin',
  Instructor: 'Instructor',
  Student: 'Student',
};

export type Role = (typeof Roles)[keyof typeof Roles];
