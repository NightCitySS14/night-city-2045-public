# NC — CitiNet Radio Channel Localization (EN)
chat-radio-ncpd = NCPD
chat-radio-traumateam = Trauma Team
chat-radio-maxtac = MAX-TAC
chat-radio-militech = Militech
chat-radio-biotechnica = Biotechnica

# CitiNet BBS
citinet-bbs-channel-public = City Net
citinet-bbs-channel-afterlife = Afterlife
citinet-bbs-channel-maelstrom = Maelstrom
citinet-bbs-channel-ncpd-dispatch = NCPD | Dispatch
citinet-bbs-channel-ncpd-detectives = NCPD | Detectives
citinet-bbs-channel-ncpd-command = NCPD | Command
citinet-bbs-channel-maxtac-tactical = MaxTac | Tactical
citinet-bbs-channel-maxtac-command = MaxTac | Command
citinet-bbs-channel-biotech-general = Biotechnica | General
citinet-bbs-channel-biotech-operatives = Biotech | Operatives
citinet-bbs-channel-biotech-command = Biotech | Command
citinet-bbs-channel-trauma-general = Trauma Team
citinet-bbs-channel-trauma-operatives = Field Operatives
citinet-bbs-channel-trauma-comms = Corporate Comms

# CitiNet Cartridge UI
citinet-cartridge-name = CitiNet
citinet-cartridge-description = Night City communication network terminal.

citinet-tab-calls = Calls
citinet-tab-group = Tactical
citinet-tab-bbs = BBS

citinet-call-initiate = Call
citinet-call-accept = Accept
citinet-call-decline = Decline
citinet-call-hangup = Hang up
citinet-call-incoming = Incoming call from {$caller}...
citinet-call-ringing = Calling {$target}...
citinet-call-flatline = FLATLINE — {$name} is down
citinet-flatline-dead = FLATLINE — {$name} KIA
citinet-flatline-critical = CRITICAL — {$name} is down
citinet-call-active = Connected — {$target}
citinet-call-no-relay = [color=red]No signal — CitiNet Relay offline[/color]
citinet-call-ping-location = {$sender} sent location: {$coords}

citinet-group-create = Create bridge
citinet-group-invite = Invite
citinet-group-leave = Leave
citinet-group-participants = Participants: {$count}/{$max}
citinet-group-flatline = [color=red][SYSTEM]: Agent {$name} disconnected. Status: CRITICAL (FLATLINE)[/color]

citinet-bbs-join = Join
citinet-bbs-leave = Leave
citinet-bbs-send = Send
citinet-bbs-password-required = Password required
citinet-bbs-enter-password = Enter access code:
citinet-bbs-wrong-password = Access denied — wrong code
citinet-bbs-no-relay = [color=red]Channel unavailable — CitiNet offline[/color]
citinet-bbs-anonymous = Anonymous
citinet-bbs-invite-received = >> You have been granted access to {$channel} by {$inviter}
citinet-bbs-invite-sent = >> Agent {$target} has been invited to {$channel}
citinet-p2p-game-chat = [CitiNet/Direct] {$sender}: {$message}
citinet-group-game-chat = [CitiNet/Bridge] {$sender}: {$message}
citinet-bbs-game-chat = [CitiNet/{$channel}] {$sender}: {$message}

# BurnerChip
citinet-burner-chip-name = Burner chip
citinet-burner-chip-description = A cheap black market chip. Provides a temporary anonymous ID that can't be traced by NCPD databases.
citinet-burner-chip-inserted = Burner chip activated. Temporary ID: {$id}
citinet-burner-chip-removed = Burner chip deactivated. Original ID restored.
citinet-burner-chip-used = This chip has already been used.
citinet-burner-chip-destroyed = Burner chip destroyed.

# CitiNet Relay
citinet-relay-name = CitiNet Relay
citinet-relay-description = A local city network relay server. Routes civilian communications — calls and BBS channels. Requires power.

citinet-sender-system = SYSTEM
citinet-sender-flatline = FLATLINE
citinet-call-busy = >> TARGET LINE BUSY. TRY AGAIN LATER.
citinet-call-connection-lost = >> CONNECTION LOST. RELAY SIGNAL DROPPED.

# CitiNet UI
citinet-ui-app-name = CitiNet
citinet-ui-screen-calls = CALLS
citinet-ui-screen-chats = CHATS
citinet-ui-screen-pool = POOL
citinet-ui-screen-contacts = CONTACTS
citinet-ui-subtitle-contact-routing = CONTACT ROUTING
citinet-ui-subtitle-available-networks = AVAILABLE NETWORKS
citinet-ui-subtitle-tactical-bridge = TACTICAL BRIDGE
citinet-ui-subtitle-active-citizens = ACTIVE CITIZENS
citinet-ui-list-direct-calls = DIRECT CALLS
citinet-ui-list-chat-rooms = CHAT ROOMS
citinet-ui-list-data-pool = DATA POOL
citinet-ui-neural-line = Agents ID #{$number}
citinet-ui-network-link-stable = LINK STABLE
citinet-ui-network-relay-lost = RELAY LOST
citinet-ui-status-online = ONLINE
citinet-ui-status-offline = OFFLINE
citinet-ui-agent-profile = AGENT PROFILE // {$number}
citinet-ui-header-p2p-communication = P2P COMMUNICATION
citinet-ui-header-public-secure-bbs = PUBLIC / SECURE BBS
citinet-ui-header-directory = DIRECTORY
citinet-ui-copy-direct-messages-voice = Direct messages and voice call.
citinet-ui-copy-channel-feed = Channel feed.
citinet-ui-copy-no-active-bridge = No active bridge.
citinet-ui-copy-live-squad-mesh = Live squad mesh.
citinet-ui-copy-citizen-directory = Citizen directory and emergency actions.
citinet-ui-placeholder-new-agent-number = NEW AGENT NUMBER...
citinet-ui-placeholder-agent-number = AGENT NUMBER...
citinet-ui-placeholder-enter-access-code = ENTER ACCESS CODE...
citinet-ui-placeholder-transmit-message = Transmit message...
citinet-ui-action-send = SEND
citinet-ui-action-unlock = UNLOCK
citinet-ui-action-execute = EXECUTE
citinet-ui-action-start-chat = START CHAT
citinet-ui-action-invite-to-channel = INVITE TO CHANNEL
citinet-ui-action-create-tactical-bridge = CREATE TACTICAL BRIDGE
citinet-ui-action-invite-agent = INVITE AGENT
citinet-ui-action-leave-bridge = LEAVE BRIDGE
citinet-ui-action-dial = DIAL
citinet-ui-action-accept = ACCEPT
citinet-ui-action-decline = DECLINE
citinet-ui-action-join-voice = JOIN VOICE
citinet-ui-action-leave-voice = LEAVE VOICE
citinet-ui-action-ping = PING
citinet-ui-action-hang-up = HANG UP
citinet-ui-action-call-police = CALL POLICE
citinet-ui-action-call-police-cooldown = CALL POLICE ({$seconds})
citinet-ui-action-call-trauma = CALL TRAUMA
citinet-ui-action-call-trauma-cooldown = CALL TRAUMA ({$seconds})
citinet-ui-empty-no-active-contacts = NO ACTIVE CONTACTS
citinet-ui-empty-no-channels-found = NO CHANNELS FOUND
citinet-ui-empty-no-players-online = NO PLAYERS ONLINE
citinet-ui-hint-select-contact = Select a contact to view traffic.
citinet-ui-voice-connected = VOICE CHANNEL // CONNECTED
citinet-ui-voice-ringing = VOICE CHANNEL // RINGING
citinet-ui-voice-incoming = VOICE CHANNEL // INCOMING
citinet-ui-voice-idle = VOICE CHANNEL // IDLE
citinet-ui-channel-linked = Channel linked.
citinet-ui-channel-access-not-established = Access not established.
citinet-ui-channel-secured-enter-code = SYSTEM // SECURED CHANNEL. ENTER ACCESS CODE.
citinet-ui-bridge-offline-header = TACTICAL BRIDGE // OFFLINE
citinet-ui-bridge-online-header = TACTICAL BRIDGE // ONLINE
citinet-ui-bridge-offline-list = BRIDGE OFFLINE
citinet-ui-group-voice-active = GROUP VOICE // ACTIVE
citinet-ui-group-voice-standby = GROUP VOICE // STANDBY
citinet-ui-group-slots = SLOTS: {$count}/{$max}
citinet-ui-group-member-alive = > {$name}
citinet-ui-group-member-flatline = X {$name} [FLATLINE]
citinet-ui-player-entry = {$name} (ID: {$number})
citinet-ui-emergency-line-online = SYSTEM // Emergency line online.
citinet-ui-emergency-ncpd = NCPD // Dispatch to your position.
citinet-ui-emergency-trauma = TRAUMA // Medical assistance.
citinet-ui-false-alarms-warning = False alarms are punishable.
