/**
 * The frontend keeps its own copy of the site config defaults so the page can render
 * before /api/settings/public answers. That copy silently rots when the backend gains a
 * field, so this test reads the C# DTO and fails when the two drift apart.
 */
import { describe, it, expect } from 'vitest'
import { readFileSync } from 'node:fs'
import { fileURLToPath, URL } from 'node:url'
import { DEFAULTS } from '../useSiteConfig'

const dtoPath = fileURLToPath(new URL('../../../../DTOs/AuthDtos.cs', import.meta.url))

function dtoFieldNames() {
  const src = readFileSync(dtoPath, 'utf8')
  const start = src.indexOf('public record PublicSiteConfigDto(')
  const body = src.slice(start, src.indexOf(');', start))
  return body
    .split(/\r?\n/)                             // the C# files use CRLF
    .slice(1)                                   // drop the record declaration line
    .map(line => line.replace(/\/\/.*/, '').trim())
    .filter(Boolean)
    // "string? LogoUrl," / "List<long> QuickDonationAmounts);"
    .map(line => line.replace(/[,)];?$/, '').trim().split(/\s+/).pop())
    .filter(name => /^[A-Z]/.test(name))
    // ASP.NET serialises these as camelCase
    .map(name => name[0].toLowerCase() + name.slice(1))
}

describe('site config contract', () => {
  const fields = dtoFieldNames()

  it('finds the fields of the API DTO', () => {
    expect(fields.length).toBeGreaterThan(50)
    expect(fields).toContain('siteNameFa')
    expect(fields).toContain('progressFlowStyle')
  })

  it('has a frontend default for every field the API sends', () => {
    const missing = fields.filter(f => !(f in DEFAULTS))
    expect(missing, `add these to DEFAULTS in useSiteConfig.js: ${missing.join(', ')}`).toEqual([])
  })

  it('has no leftover defaults the API no longer sends', () => {
    const stale = Object.keys(DEFAULTS).filter(k => !fields.includes(k))
    expect(stale, `these are not in PublicSiteConfigDto any more: ${stale.join(', ')}`).toEqual([])
  })
})
