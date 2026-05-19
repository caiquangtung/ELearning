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
  priceCents: number;
  currency: string;
  createdAt: string;
}

export interface ListCoursesRequest {
  page: number;
  pageSize: number;
  search?: string | null;
  status?: string | null;
  minPriceCents?: number | null;
  maxPriceCents?: number | null;
  sort?: string | null;
}

export interface ListTrainingClassesRequest {
  page: number;
  pageSize: number;
  courseId?: string | null;
  search?: string | null;
}

export interface ListMyOrdersRequest {
  buyerUserId: string;
  take?: number;
}

export interface ListCampaignsRequest {
  organizationId?: string | null;
  includeGlobal?: boolean;
  take?: number;
}

export interface ListQuizzesRequest {
  page: number;
  pageSize: number;
  search?: string | null;
  status?: string | null;
}

export interface GetAttemptRequest {
  userId: string;
}

export interface ListNotificationsRequest {
  page?: number;
  pageSize?: number;
  unreadOnly?: boolean;
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

export interface PromotionQuoteItemDto {
  itemType: string;
  referenceId: string;
  quantity: number;
  unitPriceCents: number;
  lineTotalCents: number;
  discountCents: number;
}

export interface PromotionQuoteDto {
  currency: string;
  subtotalCents: number;
  discountCents: number;
  totalCents: number;
  appliedCouponCode: string | null;
  items: PromotionQuoteItemDto[];
}

export interface QuoteCheckoutItemRequest {
  itemType: string;
  referenceId: string;
  quantity: number;
}

export interface QuoteCheckoutRequest {
  buyerUserId: string;
  organizationId: string | null;
  currency: string;
  items: QuoteCheckoutItemRequest[];
  couponCode: string | null;
}

export interface PromotionRuleDto {
  id: string;
  ruleType: string;
  percentOff: number;
  appliesToItemTypes: string[];
}

export interface CouponDto {
  id: string;
  campaignId: string;
  code: string;
  status: string;
  expiresUtc: string | null;
  perBuyerMaxRedemptions: number;
}

export interface CampaignDto {
  id: string;
  name: string;
  scope: string;
  organizationId: string | null;
  status: string;
  startUtc: string;
  endUtc: string | null;
  rules: PromotionRuleDto[];
  coupons: CouponDto[];
}

export interface CampaignListItemDto {
  id: string;
  name: string;
  scope: string;
  organizationId: string | null;
  status: string;
  startUtc: string;
  endUtc: string | null;
}

export interface CampaignAnalyticsDto {
  campaignId: string;
  totalRedemptions: number;
  uniqueBuyers: number;
  totalDiscountCents: number;
  lastRedeemedAtUtc: string | null;
}

export interface CreateCampaignRequest {
  name: string;
  scope: string;
  organizationId: string | null;
  startUtc: string;
  endUtc: string | null;
}

export interface AddItemPercentOffRuleRequest {
  campaignId: string; // ignored by server; provided for type symmetry
  percentOff: number;
  appliesToItemTypes: string[];
}

export interface CreateCouponRequest {
  campaignId: string; // ignored by server; provided for type symmetry
  code: string;
  expiresUtc: string | null;
  perBuyerMaxRedemptions: number;
}

export interface PreviewCampaignQuoteItemRequest {
  itemType: string;
  referenceId: string;
  quantity: number;
}

export interface PreviewCampaignQuoteRequest {
  buyerUserId: string;
  organizationId: string | null;
  currency: string;
  items: PreviewCampaignQuoteItemRequest[];
  couponCode: string | null;
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

 // Quiz DTOs
 export interface QuizDto {
   id: string;
   courseId: string | null;
   lessonId: string | null;
   title: string;
   description: string | null;
   status: string;
   timeLimitMinutes: number | null;
   passingScore: number | null;
   createdAt: string;
   updatedAt: string | null;
 }

 export interface QuizListItemDto {
   id: string;
   title: string;
   status: string;
   questionCount: number;
   createdAt: string;
 }

 export interface QuizDetailDto {
   id: string;
   courseId: string | null;
   lessonId: string | null;
   title: string;
   description: string | null;
   status: string;
   timeLimitMinutes: number | null;
   passingScore: number | null;
   createdAt: string;
   updatedAt: string | null;
   questions: QuestionDto[];
 }

 export interface QuestionDto {
   id: string;
   text: string;
   type: string; // MultipleChoice, Essay, Code
   points: number;
   sortOrder: number;
   options: QuestionOptionDto[];
 }

 export interface QuestionOptionDto {
   id: string;
   text: string;
   isCorrect: boolean;
   sortOrder: number;
 }

 export interface QuizAttemptDto {
   id: string;
   quizId: string;
   userId: string;
   startedAt: string;
   submittedAt: string | null;
   status: string; // InProgress, Submitted, Graded
   totalScore: number | null;
   createdAt: string;
 }

 export interface QuizResultDto {
   attemptId: string;
   quizId: string;
   quizTitle: string;
   totalScore: number | null;
   passingScore: number | null;
   passed: boolean;
   submittedAt: string;
   questionResults: QuestionResultDto[];
 }

 export interface QuestionResultDto {
   questionId: string;
   questionText: string;
   points: number;
   score: number | null;
   isCorrect: boolean | null;
   textAnswer: string | null;
   selectedOptionId: string | null;
 }

 export interface QuizAnalyticsDto {
   quizId: string;
   quizTitle: string;
   totalAttempts: number;
   completedAttempts: number;
   averageScore: number;
   passRate: number;
   highestScore: number;
   lowestScore: number;
 }

 // Quiz request DTOs
 export interface CreateQuizRequest {
   courseId: string | null;
   lessonId: string | null;
   title: string;
   description: string | null;
   timeLimitMinutes: number | null;
   passingScore: number | null;
 }

 export interface UpdateQuizRequest {
   title: string;
   description: string | null;
   timeLimitMinutes: number | null;
   passingScore: number | null;
 }

 export interface AddQuestionRequest {
   text: string;
   type: string; // MultipleChoice, Essay, Code
   points: number;
   sortOrder: number;
   options: AddQuestionOptionRequest[];
 }

 export interface AddQuestionOptionRequest {
   text: string;
   isCorrect: boolean;
   sortOrder: number;
 }

 export interface UpdateQuestionRequest {
   text: string;
   type: string; // MultipleChoice, Essay, Code
   points: number;
   sortOrder: number;
 }

 export interface StartAttemptRequest {
   userId: string;
 }

 export interface SubmitAttemptRequest {
   userId: string;
   answers: AnswerSubmissionRequest[];
 }

 export interface AnswerSubmissionRequest {
   questionId: string;
   selectedOptionId: string | null;
   textAnswer: string | null;
 }

export interface GradeAttemptRequest {
   grades: QuestionGradeRequest[];
 }

 export interface QuestionGradeRequest {
   questionId: string;
   score: number;
   isCorrect: boolean | null;
 }

export interface NotificationDto {
  id: string;
  userId: string;
  messageId: string | null;
  title: string;
  body: string;
  type: string;
  actionUrl: string | null;
  isRead: boolean;
  readAt: string | null;
  createdAt: string;
}

export interface UnreadNotificationCountDto {
  count: number;
}

export interface MessageDto {
  id: string;
  senderUserId: string;
  subject: string;
  body: string;
  scope: string;
  organizationId: string | null;
  courseId: string | null;
  trainingClassId: string | null;
  recipientCount: number;
  sentAt: string;
}

export interface SendAnnouncementRequest {
  recipientUserIds: string[];
  subject: string;
  body: string;
  scope: string;
  organizationId: string | null;
  courseId: string | null;
  trainingClassId: string | null;
  actionUrl: string | null;
}

export interface AdminDashboardDto {
  totalUsers: number;
  activeUsers: number;
  totalCourses: number;
  publishedCourses: number;
  totalClasses: number;
  scheduledClasses: number;
  paidOrders: number;
  pendingOrders: number;
  revenueCents: number;
  currency: string;
  certificatesIssued: number;
}

export interface StudentDashboardDto {
  userId: string;
  paidOrders: number;
  coursePurchases: number;
  classPurchases: number;
  certificatesIssued: number;
  upcomingSessions: number;
}

export interface InstructorDashboardDto {
  userId: string;
  assignedClasses: number;
  upcomingSessions: number;
  completedSessions: number;
  draftClasses: number;
  scheduledClasses: number;
}

export interface VideoAssetDto {
  id: string;
  lessonId: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  url: string;
  durationSeconds: number | null;
  uploadedAt: string;
}

export interface WatchProgressDto {
  id: string;
  videoAssetId: string;
  lessonId: string;
  userId: string;
  lastPositionSeconds: number;
  durationSeconds: number;
  watchedSeconds: number;
  progressPercent: number;
  isCompleted: boolean;
  completedAt: string | null;
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

  listCourses(request: ListCoursesRequest): Observable<PagedList<CourseListItemDto>> {
    let params = new HttpParams().set('page', request.page).set('pageSize', request.pageSize);
    if (request.search?.trim()) {
      params = params.set('search', request.search.trim());
    }
    if (request.status?.trim()) {
      params = params.set('status', request.status.trim());
    }
    if (request.minPriceCents !== null && request.minPriceCents !== undefined) {
      params = params.set('minPriceCents', request.minPriceCents);
    }
    if (request.maxPriceCents !== null && request.maxPriceCents !== undefined) {
      params = params.set('maxPriceCents', request.maxPriceCents);
    }
    if (request.sort?.trim()) {
      params = params.set('sort', request.sort.trim());
    }
    return this.http.get<PagedList<CourseListItemDto>>(`${this.base}/courses`, { params });
  }

  getCourse(id: string): Observable<CourseDetailDto> {
    return this.http.get<CourseDetailDto>(`${this.base}/courses/${id}`);
  }

  createCourse(body: { title: string; description: string | null }): Observable<CourseListItemDto> {
    return this.http.post<CourseListItemDto>(`${this.base}/courses`, body);
  }

  listTrainingClasses(request: ListTrainingClassesRequest): Observable<PagedList<TrainingClassListItemDto>> {
    let params = new HttpParams().set('page', request.page).set('pageSize', request.pageSize);
    if (request.courseId) {
      params = params.set('courseId', request.courseId);
    }
    if (request.search?.trim()) {
      params = params.set('search', request.search.trim());
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

  listMyOrders(request: ListMyOrdersRequest): Observable<OrderListItemDto[]> {
    const params = new HttpParams()
      .set('buyerUserId', request.buyerUserId)
      .set('take', String(request.take ?? 50));
    return this.http.get<OrderListItemDto[]>(`${this.base}/orders/my`, { params });
  }

  payOrder(id: string): Observable<OrderDto> {
    return this.http.post<OrderDto>(`${this.base}/orders/${id}/pay`, {});
  }

  getOrderInvoice(orderId: string): Observable<InvoiceDto> {
    return this.http.get<InvoiceDto>(`${this.base}/orders/${orderId}/invoice`);
  }

  quoteCheckout(body: QuoteCheckoutRequest): Observable<PromotionQuoteDto> {
    return this.http.post<PromotionQuoteDto>(`${this.base}/checkout/quote`, body);
  }

  listCampaigns(request: ListCampaignsRequest): Observable<CampaignListItemDto[]> {
    let params = new HttpParams()
      .set('includeGlobal', String(request.includeGlobal ?? true))
      .set('take', String(request.take ?? 50));
    if (request.organizationId) params = params.set('organizationId', request.organizationId);
    return this.http.get<CampaignListItemDto[]>(`${this.base}/campaigns`, { params });
  }

  getCampaign(id: string): Observable<CampaignDto> {
    return this.http.get<CampaignDto>(`${this.base}/campaigns/${id}`);
  }

  getCampaignAnalytics(id: string): Observable<CampaignAnalyticsDto> {
    return this.http.get<CampaignAnalyticsDto>(`${this.base}/campaigns/${id}/analytics`);
  }

  createCampaign(body: CreateCampaignRequest): Observable<CampaignDto> {
    return this.http.post<CampaignDto>(`${this.base}/campaigns`, body);
  }

  addCampaignRule(campaignId: string, body: Omit<AddItemPercentOffRuleRequest, 'campaignId'>): Observable<CampaignDto> {
    return this.http.post<CampaignDto>(`${this.base}/campaigns/${campaignId}/rules`, body);
  }

  createCampaignCoupon(campaignId: string, body: Omit<CreateCouponRequest, 'campaignId'>): Observable<CampaignDto> {
    return this.http.post<CampaignDto>(`${this.base}/campaigns/${campaignId}/coupons`, body);
  }

  previewCampaign(campaignId: string, body: PreviewCampaignQuoteRequest): Observable<PromotionQuoteDto> {
     return this.http.post<PromotionQuoteDto>(`${this.base}/campaigns/${campaignId}/preview`, body);
   }

   // Quiz API methods
   listQuizzes(request: ListQuizzesRequest): Observable<PagedList<QuizListItemDto>> {
     let params = new HttpParams().set('page', request.page).set('pageSize', request.pageSize);
     if (request.search?.trim()) {
       params = params.set('search', request.search.trim());
     }
     if (request.status?.trim()) {
       params = params.set('status', request.status.trim());
     }
     return this.http.get<PagedList<QuizListItemDto>>(`${this.base}/quizzes`, { params });
   }

   getQuiz(id: string): Observable<QuizDetailDto> {
     return this.http.get<QuizDetailDto>(`${this.base}/quizzes/${id}`);
   }

   createQuiz(body: CreateQuizRequest): Observable<QuizDto> {
     return this.http.post<QuizDto>(`${this.base}/quizzes`, body);
   }

   updateQuiz(id: string, body: UpdateQuizRequest): Observable<QuizDto> {
     return this.http.put<QuizDto>(`${this.base}/quizzes/${id}`, body);
   }

   deleteQuiz(id: string): Observable<unknown> {
     return this.http.delete(`${this.base}/quizzes/${id}`, { responseType: 'text' });
   }

   publishQuiz(id: string): Observable<QuizDto> {
     return this.http.post<QuizDto>(`${this.base}/quizzes/${id}/publish`, {});
   }

   addQuestion(quizId: string, body: AddQuestionRequest): Observable<QuestionDto> {
     return this.http.post<QuestionDto>(`${this.base}/quizzes/${quizId}/questions`, body);
   }

   updateQuestion(quizId: string, questionId: string, body: UpdateQuestionRequest): Observable<QuestionDto> {
     return this.http.put<QuestionDto>(`${this.base}/quizzes/${quizId}/questions/${questionId}`, body);
   }

   removeQuestion(quizId: string, questionId: string): Observable<unknown> {
     return this.http.delete(`${this.base}/quizzes/${quizId}/questions/${questionId}`, { responseType: 'text' });
   }

   startAttempt(quizId: string, body: StartAttemptRequest): Observable<QuizAttemptDto> {
     return this.http.post<QuizAttemptDto>(`${this.base}/quizzes/${quizId}/attempts`, body);
   }

   getAttempt(attemptId: string, request: GetAttemptRequest): Observable<QuizResultDto> {
     const params = new HttpParams().set('userId', request.userId);
     return this.http.get<QuizResultDto>(`${this.base}/quizzes/attempts/${attemptId}`, { params });
   }

   submitAttempt(attemptId: string, body: SubmitAttemptRequest): Observable<QuizAttemptDto> {
     return this.http.post<QuizAttemptDto>(`${this.base}/quizzes/attempts/${attemptId}/submit`, body);
   }

   gradeAttempt(attemptId: string, body: GradeAttemptRequest): Observable<QuizAttemptDto> {
     return this.http.post<QuizAttemptDto>(`${this.base}/quizzes/attempts/${attemptId}/grade`, body);
   }

   getQuizAnalytics(id: string): Observable<QuizAnalyticsDto> {
     return this.http.get<QuizAnalyticsDto>(`${this.base}/quizzes/${id}/analytics`);
   }

  listNotifications(request: ListNotificationsRequest = {}): Observable<PagedList<NotificationDto>> {
    const params = new HttpParams()
      .set('page', request.page ?? 1)
      .set('pageSize', request.pageSize ?? 20)
      .set('unreadOnly', String(request.unreadOnly ?? false));
    return this.http.get<PagedList<NotificationDto>>(`${this.base}/notifications`, { params });
  }

  getUnreadNotificationCount(): Observable<UnreadNotificationCountDto> {
    return this.http.get<UnreadNotificationCountDto>(`${this.base}/notifications/unread-count`);
  }

  markNotificationRead(id: string): Observable<NotificationDto> {
    return this.http.post<NotificationDto>(`${this.base}/notifications/${id}/read`, {});
  }

  sendAnnouncement(body: SendAnnouncementRequest): Observable<MessageDto> {
    return this.http.post<MessageDto>(`${this.base}/notifications/announcements`, body);
  }

  getAdminDashboard(): Observable<AdminDashboardDto> {
    return this.http.get<AdminDashboardDto>(`${this.base}/reports/dashboard/admin`);
  }

  getStudentDashboard(): Observable<StudentDashboardDto> {
    return this.http.get<StudentDashboardDto>(`${this.base}/reports/dashboard/student`);
  }

  getInstructorDashboard(): Observable<InstructorDashboardDto> {
    return this.http.get<InstructorDashboardDto>(`${this.base}/reports/dashboard/instructor`);
  }

  getLessonVideo(lessonId: string): Observable<VideoAssetDto> {
    return this.http.get<VideoAssetDto>(`${this.base}/videos/lessons/${lessonId}`);
  }

  uploadVideo(
    courseId: string,
    sectionId: string,
    lessonId: string,
    file: File,
    durationSeconds?: number | null,
  ): Observable<VideoAssetDto> {
    const form = new FormData();
    form.append('file', file);
    if (durationSeconds) {
      form.append('durationSeconds', String(durationSeconds));
    }
    return this.http.post<VideoAssetDto>(
      `${this.base}/videos/courses/${courseId}/sections/${sectionId}/lessons/${lessonId}`,
      form,
    );
  }

  trackVideoProgress(
    videoAssetId: string,
    body: { positionSeconds: number; durationSeconds: number; watchedSeconds: number },
  ): Observable<WatchProgressDto> {
    return this.http.post<WatchProgressDto>(`${this.base}/videos/${videoAssetId}/progress`, body);
  }

  markVideoComplete(videoAssetId: string): Observable<WatchProgressDto> {
    return this.http.post<WatchProgressDto>(`${this.base}/videos/${videoAssetId}/complete`, {});
  }
 }
