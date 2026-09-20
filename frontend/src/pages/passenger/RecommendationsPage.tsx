import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { isAxiosError } from 'axios'
import { toast } from 'sonner'
import { Lightbulb } from 'lucide-react'
import { recommendationsApi } from '@/api/recommendations'
import type { RecommendationStatus } from '@/types'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { Badge } from '@/components/ui/badge'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { LoadingSpinner } from '@/components/LoadingSpinner'

const schema = z.object({
  category: z.enum(['FeatureIdea', 'SafetySuggestion', 'UsabilityFeedback', 'BugReport', 'Other']),
  subject: z.string().min(5, 'Give your recommendation a short title').max(120, 'Keep the subject under 120 characters'),
  message: z.string().min(20, 'Please provide at least 20 characters of detail').max(2000, 'Keep your message under 2000 characters'),
})
type FormData = z.infer<typeof schema>

const categories: { value: FormData['category']; label: string }[] = [
  { value: 'FeatureIdea', label: 'Feature Idea' },
  { value: 'SafetySuggestion', label: 'Safety Suggestion' },
  { value: 'UsabilityFeedback', label: 'Usability Feedback' },
  { value: 'BugReport', label: 'Bug Report' },
  { value: 'Other', label: 'Other' },
]

const categoryLabels = Object.fromEntries(categories.map((c) => [c.value, c.label]))

const statusVariants: Record<RecommendationStatus, 'secondary' | 'default' | 'success' | 'destructive'> = {
  Pending: 'secondary',
  Reviewed: 'default',
  Planned: 'success',
  Declined: 'destructive',
}

export function RecommendationsPage() {
  const queryClient = useQueryClient()

  const { register, handleSubmit, setValue, reset, formState: { errors, isSubmitting } } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  const { data, isLoading, isError } = useQuery({
    queryKey: ['my-recommendations'],
    queryFn: () => recommendationsApi.getMine({ pageSize: 20 }),
  })

  const mutation = useMutation({
    mutationFn: recommendationsApi.create,
    onSuccess: () => {
      toast.success('Thanks! Your recommendation has been sent to the RydrSafe team.')
      reset()
      queryClient.invalidateQueries({ queryKey: ['my-recommendations'] })
    },
    onError: (err) => {
      const detail = isAxiosError<{ title?: string }>(err) ? err.response?.data?.title : undefined
      toast.error(detail ?? err.message ?? 'Failed to submit your recommendation. Please try again.')
    },
  })

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Recommendations</h1>
        <p className="text-gray-500 mt-1">Tell us how we can make RydrSafe safer and easier to use.</p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Lightbulb className="h-5 w-5 text-blue-600" />
            Share a Recommendation
          </CardTitle>
          <CardDescription>
            Ideas, safety suggestions and feedback go straight to the RydrSafe team. To report a driver, use the Report
            Driver page instead.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit((d) => mutation.mutate(d))} className="space-y-5">
            <div className="space-y-1">
              <Label>Category</Label>
              <Select onValueChange={(v) => setValue('category', v as FormData['category'])}>
                <SelectTrigger><SelectValue placeholder="Select category" /></SelectTrigger>
                <SelectContent>
                  {categories.map((c) => <SelectItem key={c.value} value={c.value}>{c.label}</SelectItem>)}
                </SelectContent>
              </Select>
              {errors.category && <p className="text-xs text-red-500">{errors.category.message}</p>}
            </div>

            <div className="space-y-1">
              <Label>Subject</Label>
              <Input placeholder="e.g. Let me share my trip with an emergency contact" {...register('subject')} />
              {errors.subject && <p className="text-xs text-red-500">{errors.subject.message}</p>}
            </div>

            <div className="space-y-1">
              <Label>Your Recommendation</Label>
              <Textarea rows={5} placeholder="Describe your idea and how it would help…" {...register('message')} />
              {errors.message && <p className="text-xs text-red-500">{errors.message.message}</p>}
            </div>

            <Button type="submit" className="w-full" disabled={isSubmitting || mutation.isPending}>
              {mutation.isPending ? 'Submitting…' : 'Submit Recommendation'}
            </Button>
          </form>
        </CardContent>
      </Card>

      <div>
        <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-3">Your Recommendations</h2>

        {isLoading && <LoadingSpinner className="py-12" />}

        {isError && (
          <Card>
            <CardContent className="py-12 text-center text-gray-500">
              We couldn't load your recommendations right now. Please try again later.
            </CardContent>
          </Card>
        )}

        {data?.items.length === 0 && (
          <Card>
            <CardContent className="py-12 text-center text-gray-500">
              You haven't sent any recommendations yet.
            </CardContent>
          </Card>
        )}

        <div className="space-y-3">
          {data?.items.map((item) => (
            <Card key={item.id}>
              <CardContent className="pt-4 pb-4">
                <div className="flex items-start justify-between gap-4">
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-3 mb-2">
                      <h3 className="font-semibold text-gray-900 dark:text-white truncate">{item.subject}</h3>
                      <Badge variant={statusVariants[item.status]}>{item.status}</Badge>
                    </div>
                    <p className="text-sm text-gray-500">{item.message}</p>
                    <p className="text-xs text-gray-400 mt-2">
                      {categoryLabels[item.category] ?? item.category} · {new Date(item.createdAt).toLocaleDateString()}
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      </div>
    </div>
  )
}
