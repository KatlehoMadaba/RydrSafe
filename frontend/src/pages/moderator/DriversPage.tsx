import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { driversApi } from '@/api/drivers'
import { Card, CardContent } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Table, TableHeader, TableBody, TableRow, TableHead, TableCell } from '@/components/ui/table'
import { RiskBadge } from '@/components/RiskBadge'
import { RiskScore } from '@/components/RiskScore'
import { LoadingSpinner } from '@/components/LoadingSpinner'
import { ErrorState } from '@/components/ErrorState'
import { Search, Car } from 'lucide-react'

// Moderator density is compact and tabular — fast scanning across many
// drivers, not a card-per-driver consumer layout.
export function ModeratorDriversPage() {
  const [search, setSearch] = useState('')

  const { data, isLoading, isError, refetch } = useQuery({
    queryKey: ['drivers', search],
    queryFn: () => driversApi.getAll({ search, pageSize: 50 }),
  })

  return (
    <div className="space-y-6">
      <h1 className="font-display text-2xl font-bold text-foreground">Drivers</h1>

      <div className="relative">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-subtle" />
        <Input className="pl-9" placeholder="Search by name, plate, or phone…" value={search} onChange={(e) => setSearch(e.target.value)} />
      </div>

      {isLoading && <LoadingSpinner className="py-12" />}
      {isError && <ErrorState message="Couldn't load drivers." onRetry={() => refetch()} />}

      {!isLoading && !isError && data?.items.length === 0 && (
        <Card>
          <CardContent className="py-12 text-center">
            <Car className="h-10 w-10 text-subtle mx-auto mb-3" />
            <p className="text-muted-foreground">{search ? 'No drivers match your search.' : 'No drivers in the database yet.'}</p>
          </CardContent>
        </Card>
      )}

      {!isLoading && !isError && data && data.items.length > 0 && (
        <Card>
          <CardContent className="p-0">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Driver</TableHead>
                  <TableHead>Vehicle</TableHead>
                  <TableHead>Reports</TableHead>
                  <TableHead>Risk score</TableHead>
                  <TableHead>Status</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {data.items.map((driver) => (
                  <TableRow key={driver.id}>
                    <TableCell>
                      <p className="font-medium text-foreground">{driver.driverName}</p>
                    </TableCell>
                    <TableCell className="text-muted-foreground">{driver.registrationNumber ?? '—'}</TableCell>
                    <TableCell className="text-muted-foreground">{driver.reportCount}</TableCell>
                    <TableCell className="w-40">
                      <RiskScore score={driver.riskScore} />
                    </TableCell>
                    <TableCell>
                      <RiskBadge state={driver.status} />
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      )}
    </div>
  )
}
