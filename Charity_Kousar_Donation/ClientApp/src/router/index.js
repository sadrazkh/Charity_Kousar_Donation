import { createRouter, createWebHistory } from 'vue-router'
import { getToken } from '@/api/client'

const routes = [
  { path: '/', name: 'home', component: () => import('@/views/HomeView.vue') },
  { path: '/c/:slug', name: 'campaign', component: () => import('@/views/CampaignView.vue') },
  { path: '/payment/success', name: 'pay-success', component: () => import('@/views/PaymentResultView.vue'), props: { success: true } },
  { path: '/payment/failed', name: 'pay-failed', component: () => import('@/views/PaymentResultView.vue'), props: { success: false } },
  { path: '/payment/test', name: 'pay-test', component: () => import('@/views/TestPaymentView.vue') },
  { path: '/admin/login', name: 'admin-login', component: () => import('@/views/admin/AdminLogin.vue') },
  {
    path: '/admin',
    component: () => import('@/views/admin/AdminLayout.vue'),
    meta: { requiresAuth: true },
    children: [
      { path: '', name: 'admin-dashboard', component: () => import('@/views/admin/AdminDashboard.vue') },
      { path: 'home', name: 'admin-home', component: () => import('@/views/admin/AdminHome.vue') },
      { path: 'campaigns', name: 'admin-campaigns', component: () => import('@/views/admin/AdminCampaigns.vue') },
      { path: 'campaigns/new', name: 'admin-campaign-new', component: () => import('@/views/admin/AdminCampaignEditor.vue') },
      { path: 'campaigns/:id/edit', name: 'admin-campaign-edit', component: () => import('@/views/admin/AdminCampaignEditor.vue') },
      { path: 'donations', name: 'admin-donations', component: () => import('@/views/admin/AdminDonations.vue') },
      { path: 'settings', name: 'admin-settings', component: () => import('@/views/admin/AdminSettings.vue') }
    ]
  }
]

const router = createRouter({ history: createWebHistory(), routes })

router.beforeEach((to) => {
  if (to.matched.some(r => r.meta.requiresAuth) && !getToken())
    return { name: 'admin-login', query: { redirect: to.fullPath } }
})

export default router
