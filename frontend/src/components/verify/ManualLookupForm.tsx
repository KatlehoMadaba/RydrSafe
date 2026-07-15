import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

interface ManualLookupFormProps {
  regNumber: string
  driverName: string
  phoneNumber: string
  onRegNumberChange: (value: string) => void
  onDriverNameChange: (value: string) => void
  onPhoneNumberChange: (value: string) => void
}

export function ManualLookupForm({
  regNumber,
  driverName,
  phoneNumber,
  onRegNumberChange,
  onDriverNameChange,
  onPhoneNumberChange,
}: ManualLookupFormProps) {
  return (
    <div className="space-y-3">
      <div className="space-y-1">
        <Label htmlFor="regNumber">
          Registration Number <span className="text-highrisk">*</span>
        </Label>
        <Input
          id="regNumber"
          placeholder="e.g. GP12ABGP"
          value={regNumber}
          onChange={(e) => onRegNumberChange(e.target.value.toUpperCase())}
        />
      </div>
      <div className="space-y-1">
        <Label htmlFor="driverName">
          Driver Name <span className="text-subtle text-xs">(optional)</span>
        </Label>
        <Input id="driverName" placeholder="e.g. John Smith" value={driverName} onChange={(e) => onDriverNameChange(e.target.value)} />
      </div>
      <div className="space-y-1">
        <Label htmlFor="phoneNumber">
          Phone Number <span className="text-subtle text-xs">(optional)</span>
        </Label>
        <Input id="phoneNumber" placeholder="e.g. 0821234567" value={phoneNumber} onChange={(e) => onPhoneNumberChange(e.target.value)} />
      </div>
    </div>
  )
}
