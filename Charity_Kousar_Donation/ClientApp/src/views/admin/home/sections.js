// The blocks that make up the public home page, shared by the order editor and
// the layout preview. `label` is an i18n key under `homeEditor`.
export const SECTIONS = [
  { id: 'hero', label: 'secHero', icon: 'M12 20h9M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4z' },
  { id: 'featured', label: 'secFeatured', icon: 'M12 2l2.9 6.1 6.6.9-4.8 4.6 1.2 6.6L12 17.8 6.1 20.8l1.2-6.6L2.5 9l6.6-.9L12 2z' },
  { id: 'campaigns', label: 'secCampaigns', icon: 'M3 7a2 2 0 0 1 2-2h4l2 2h8a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z' },
  { id: 'donors', label: 'secDonors', icon: 'M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2M9 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8M23 21v-2a4 4 0 0 0-3-3.9M16 3.1a4 4 0 0 1 0 7.8' }
]

export const KNOWN = SECTIONS.map(s => s.id)

export const PRESETS = [
  { id: 'classic', label: 'presetClassic', order: ['hero', 'featured', 'campaigns', 'donors'] },
  { id: 'featured', label: 'presetFeatured', order: ['featured', 'hero', 'campaigns', 'donors'] },
  { id: 'donations', label: 'presetDonations', order: ['hero', 'campaigns', 'donors'] },
  { id: 'minimal', label: 'presetMinimal', order: ['hero', 'campaigns'] }
]

export const secIcon = (id) =>
  SECTIONS.find(s => s.id === id)?.icon || 'M4 6h16M4 12h16M4 18h16'

export const secKey = (id) =>
  'homeEditor.' + (SECTIONS.find(s => s.id === id)?.label || 'secHero')
