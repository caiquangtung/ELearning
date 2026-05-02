import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedList } from '../models/paged-list.model';

export interface OrganizationDto {
  id: string;
  name: string;
  slug: string;
  status: string;
}

export interface OrganizationMemberDto {
  id: string;
  userId: string;
  departmentId: string | null;
  orgRole: string;
  joinedAt: string;
}

export interface OrganizationDetailDto {
  organization: OrganizationDto;
  members: OrganizationMemberDto[];
}

export interface CourseListItemDto {
  id: string;
  title: string;
  status: string;
  createdAt: string;
}

export interface ContentAssetDto {
  id: string;
  assetType: number;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  url: string;
  uploadedAt: string;
}

export interface CourseLessonDetailDto {
  id: string;
  title: string;
  sortOrder: number;
  content: string | null;
  assets: ContentAssetDto[];
}

export interface CourseSectionDetailDto {
  id: string;
  title: string;
  sortOrder: number;
  lessons: CourseLessonDetailDto[];
}

export interface CourseDetailDto {
  id: string;
  title: string;
  description: string | null;
  status: string;
  priceCents: number;
  currency: string;
  createdAt: string;
  updatedAt: string | null;
  sections: CourseSectionDetailDto[];
}

export interface TrainingClassListItemDto {
  id: string;
  courseId: string;
  title: string;
  status: string;
  maxLearners: number;
  createdAt: string;
}

export interface ClassInstructorDto {
  userId: string;
  assignedAt: string;
}

export interface ClassSessionDto {
  id: string;
  title: string;
  sessionType: string;
  startUtc: string;
  endUtc: string;
  location: string | null;
  zoomMeetingId: string | null;
  zoomJoinUrl: string | null;
  status: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface TrainingClassDetailDto {
  id: string;
  courseId: string;
  title: string;
  maxLearners: number;
  priceCents: number;
  currency: string;
  status: string;
  createdAt: string;
  updatedAt: string | null;
  instructors: ClassInstructorDto[];
  sessions: ClassSessionDto[];
}

export interface LicensePoolListItemDto {
  id: string;
  organizationId: string;
  name: string;
  totalSeats: number;
  activeSeats: number;
  availableSeats: number;
  seatPriceCents: number;
  currency: string;
  expiresAt: string | null;
  createdAt: string;
}

export interface LicenseAssignmentDto {
  userId: string;
  assignedAt: string;
  revokedAt: string | null;
}

export interface LicensePoolDetailDto {
  id: string;
  organizationId: string;
  name: string;
  totalSeats: number;
  activeSeats: number;
  availableSeats: number;
  seatPriceCents: number;
  currency: string;
  expiresAt: string | null;
  createdAt: string;
  assignments: LicenseAssignmentDto[];
}

export interface OrderItemDto {
  referenceId: string;
  itemType: string;
  quantity: number;
  unitPriceCents: number;
  lineTotalCents: number;
  currency: string;
}

export interface OrderDto {
  id: string;
  buyerUserId: string;
  organizationId: string | null;
  status: string;
  currency: string;
  subtotalCents: number;
  discountCents: number;
  totalCents: number;
  createdAt: string;
  updatedAt: string | null;
  checkoutExpiresAtUtc: string | null;
  items: OrderItemDto[];
}

export interface OrderListItemDto {
  id: string;
  status: string;
  currency: string;
  totalCents: number;
  createdAt: string;
}

export interface InvoiceDto {
  id: string;
  orderId: string;
  invoiceNumber: string;
  currency: string;
  totalCents: number;
  issuedAt: string;
}

export interface CreateOrderItemRequest {
  itemType: string;
  referenceId: string;
  quantity: number;
  unitPriceCents: number;
}

export interface CreateOrderRequest {
  buyerUserId: string;
  organizationId: string | null;
  currency: string;
  items: CreateOrderItemRequest[];
  discountCents?: number;
}

export interface LicenseUsageReportDto {
  licensePoolId: string;
  totalSeats: number;
  activeSeats: number;
  availableSeats: number;
}

@Injectable({ providedIn: 'root' })
export class LmsApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/v1`;

  listOrganizations(): Observable<OrganizationDto[]> {
    return this.http.get<OrganizationDto[]>(`${this.base}/organizations`);
  }

  getOrganization(id: string): Observable<OrganizationDetailDto> {
    return this.http.get<OrganizationDetailDto>(`${this.base}/organizations/${id}`);
  }

  addMember(
    orgId: string,
    body: { userId: string; orgRole: string; departmentId: string | null },
  ): Observable<OrganizationMemberDto> {
    return this.http.post<OrganizationMemberDto>(`${this.base}/organizations/${orgId}/members`, body);
  }

  createOrganization(body: { name: string; slug?: string | null }): Observable<OrganizationDto> {
    const payload: { name: string; slug?: string } = { name: body.name };
    if (body.slug?.trim()) {
      payload.slug = body.slug.trim();
    }
    return this.http.post<OrganizationDto>(`${this.base}/organizations`, payload);
  }

  listCourses(
    page: number,
    pageSize: number,
    search?: string,
    status?: string,
  ): Observable<PagedList<CourseListItemDto>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search?.trim()) {
      params = params.set('search', search.trim());
    }
    if (status?.trim()) {
      params = params.set('status', status.trim());
    }
    return this.http.get<PagedList<CourseListItemDto>>(`${this.base}/courses`, { params });
  }

  getCourse(id: string): Observable<CourseDetailDto> {
    return this.http.get<CourseDetailDto>(`${this.base}/courses/${id}`);
  }

  createCourse(body: { title: string; description: string | null }): Observable<CourseListItemDto> {
    return this.http.post<CourseListItemDto>(`${this.base}/courses`, body);
  }

  listTrainingClasses(
    page: number,
    pageSize: number,
    courseId?: string,
    search?: string,
  ): Observable<PagedList<TrainingClassListItemDto>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (courseId) {
      params = params.set('courseId', courseId);
    }
    if (search?.trim()) {
      params = params.set('search', search.trim());
    }
    return this.http.get<PagedList<TrainingClassListItemDto>>(`${this.base}/training-classes`, { params });
  }

  getTrainingClass(id: string): Observable<TrainingClassDetailDto> {
    return this.http.get<TrainingClassDetailDto>(`${this.base}/training-classes/${id}`);
  }

  createTrainingClass(body: { courseId: string; title: string; maxLearners: number }): Observable<TrainingClassListItemDto> {
    return this.http.post<TrainingClassListItemDto>(`${this.base}/training-classes`, body);
  }

  scheduleSession(
    classId: string,
    body: { title: string; sessionType: string; startUtc: string; endUtc: string; location: string | null },
  ): Observable<ClassSessionDto> {
    return this.http.post<ClassSessionDto>(`${this.base}/training-classes/${classId}/sessions`, body);
  }

  updateSession(
    classId: string,
    sessionId: string,
    body: { title: string; sessionType: string; startUtc: string; endUtc: string; location: string | null },
  ): Observable<ClassSessionDto> {
    return this.http.put<ClassSessionDto>(
      `${this.base}/training-classes/${classId}/sessions/${sessionId}`,
      body,
    );
  }

  cancelSession(classId: string, sessionId: string): Observable<unknown> {
    return this.http.post(`${this.base}/training-classes/${classId}/sessions/${sessionId}/cancel`, {}, { responseType: 'text' });
  }

  assignInstructor(classId: string, userId: string): Observable<unknown> {
    return this.http.post(`${this.base}/training-classes/${classId}/instructors`, { userId }, { responseType: 'text' });
  }

  listLicensePools(organizationId: string): Observable<LicensePoolListItemDto[]> {
    return this.http.get<LicensePoolListItemDto[]>(`${this.base}/organizations/${organizationId}/license-pools`);
  }

  createLicensePool(
    organizationId: string,
    body: { name: string; totalSeats: number; expiresAt?: string | null },
  ): Observable<LicensePoolDetailDto> {
    return this.http.post<LicensePoolDetailDto>(`${this.base}/organizations/${organizationId}/license-pools`, body);
  }

  getLicensePool(id: string): Observable<LicensePoolDetailDto> {
    return this.http.get<LicensePoolDetailDto>(`${this.base}/license-pools/${id}`);
  }

  getLicensePoolUsage(id: string): Observable<LicenseUsageReportDto> {
    return this.http.get<LicenseUsageReportDto>(`${this.base}/license-pools/${id}/usage`);
  }

  assignLicense(poolId: string, userId: string): Observable<LicenseUsageReportDto> {
    return this.http.post<LicenseUsageReportDto>(`${this.base}/license-pools/${poolId}/assignments`, { userId });
  }

  revokeLicense(poolId: string, userId: string): Observable<LicenseUsageReportDto> {
    return this.http.delete<LicenseUsageReportDto>(`${this.base}/license-pools/${poolId}/assignments/${userId}`);
  }

  createOrder(body: CreateOrderRequest): Observable<OrderDto> {
    return this.http.post<OrderDto>(`${this.base}/orders`, body);
  }

  getOrder(id: string): Observable<OrderDto> {
    return this.http.get<OrderDto>(`${this.base}/orders/${id}`);
  }

  listMyOrders(buyerUserId: string, take = 50): Observable<OrderListItemDto[]> {
    const params = new HttpParams().set('buyerUserId', buyerUserId).set('take', String(take));
    return this.http.get<OrderListItemDto[]>(`${this.base}/orders/my`, { params });
  }

  payOrder(id: string): Observable<OrderDto> {
    return this.http.post<OrderDto>(`${this.base}/orders/${id}/pay`, {});
  }

  getOrderInvoice(orderId: string): Observable<InvoiceDto> {
    return this.http.get<InvoiceDto>(`${this.base}/orders/${orderId}/invoice`);
  }
}
